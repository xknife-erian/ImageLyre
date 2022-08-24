﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ImageLyre.UI.Controls.Win
{
    [TemplateVisualState(GroupName = PRESENTATION_GROUP, Name = NORMAL_STATE)]
    [TemplateVisualState(GroupName = PRESENTATION_GROUP, Name = DEFAULT_TRANSITION_STATE)]
    [TemplatePart(Name = PREVIOUS_CONTENT_PRESENTATION_SITE_PART_NAME, Type = typeof(ContentControl))]
    [TemplatePart(Name = CURRENT_CONTENT_PRESENTATION_SITE_PART_NAME, Type = typeof(ContentControl))]
    public class TransitioningContentControl : ContentControl
    {
        private const string PRESENTATION_GROUP = "PresentationStates";

        private const string NORMAL_STATE = "Normal";

        public const string DEFAULT_TRANSITION_STATE = "DefaultTransition";

        internal const string PREVIOUS_CONTENT_PRESENTATION_SITE_PART_NAME = "PreviousContentPresentationSite";

        internal const string CURRENT_CONTENT_PRESENTATION_SITE_PART_NAME = "CurrentContentPresentationSite";

        public static readonly DependencyProperty IsTransitioningProperty =
            DependencyProperty.Register(
                "IsTransitioning",
                typeof(bool),
                typeof(TransitioningContentControl),
                new PropertyMetadata(OnIsTransitioningPropertyChanged));

        public static readonly DependencyProperty TransitionProperty =
            DependencyProperty.Register(
                "Transition",
                typeof(string),
                typeof(TransitioningContentControl),
                new PropertyMetadata(DEFAULT_TRANSITION_STATE, OnTransitionPropertyChanged));

        public static readonly DependencyProperty RestartTransitionOnContentChangeProperty =
            DependencyProperty.Register(
                "RestartTransitionOnContentChange",
                typeof(bool),
                typeof(TransitioningContentControl),
                new PropertyMetadata(false, OnRestartTransitionOnContentChangePropertyChanged));

        private bool _allowIsTransitioningWrite;
        private Storyboard _currentTransition;

        public TransitioningContentControl()
        {
            DefaultStyleKey = typeof(TransitioningContentControl);
        }

        private Storyboard CurrentTransition
        {
            get => _currentTransition;
            set
            {
                // decouple event
                if (_currentTransition != null) _currentTransition.Completed -= OnTransitionCompleted;

                _currentTransition = value;

                if (_currentTransition != null) _currentTransition.Completed += OnTransitionCompleted;
            }
        }

        private ContentPresenter CurrentContentPresentationSite { get; set; }

        private ContentPresenter PreviousContentPresentationSite { get; set; }

        public bool IsTransitioning
        {
            get => (bool) GetValue(IsTransitioningProperty);
            private set
            {
                _allowIsTransitioningWrite = true;
                SetValue(IsTransitioningProperty, value);
                _allowIsTransitioningWrite = false;
            }
        }

        public string Transition
        {
            get => GetValue(TransitionProperty) as string;
            set => SetValue(TransitionProperty, value);
        }

        public bool RestartTransitionOnContentChange
        {
            get => (bool) GetValue(RestartTransitionOnContentChangeProperty);
            set => SetValue(RestartTransitionOnContentChangeProperty, value);
        }

        #region Events

        public event RoutedEventHandler TransitionCompleted;

        #endregion Events

        public override void OnApplyTemplate()
        {
            if (IsTransitioning) AbortTransition();

            base.OnApplyTemplate();

            PreviousContentPresentationSite =
                GetTemplateChild(PREVIOUS_CONTENT_PRESENTATION_SITE_PART_NAME) as ContentPresenter;
            CurrentContentPresentationSite =
                GetTemplateChild(CURRENT_CONTENT_PRESENTATION_SITE_PART_NAME) as ContentPresenter;

            if (CurrentContentPresentationSite != null) CurrentContentPresentationSite.Content = Content;

            // hookup currenttransition
            var transition = GetStoryboard(Transition);
            CurrentTransition = transition;
            if (transition == null)
            {
                var invalidTransition = Transition;
                // revert to default
                Transition = DEFAULT_TRANSITION_STATE;

                throw new ArgumentException(
                    "TransitioningContentControl_TransitionNotFound");
            }

            VisualStateManager.GoToState(this, NORMAL_STATE, false);
            VisualStateManager.GoToState(this, Transition, true);
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            StartTransition(oldContent, newContent);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newContent",
            Justification = "Should be used in the future.")]
        private void StartTransition(object oldContent, object newContent)
        {
            // both presenters must be available, otherwise a transition is useless.
            if (CurrentContentPresentationSite != null && PreviousContentPresentationSite != null)
            {
                CurrentContentPresentationSite.Content = newContent;

                PreviousContentPresentationSite.Content = oldContent;

                // and start a new transition
                if (!IsTransitioning || RestartTransitionOnContentChange)
                {
                    IsTransitioning = true;
                    VisualStateManager.GoToState(this, NORMAL_STATE, false);
                    VisualStateManager.GoToState(this, Transition, true);
                }
            }
        }

        private void OnTransitionCompleted(object sender, EventArgs e)
        {
            AbortTransition();

            var handler = TransitionCompleted;
            if (handler != null) handler(this, new RoutedEventArgs());
        }

        public void AbortTransition()
        {
            // go to normal state and release our hold on the old content.
            VisualStateManager.GoToState(this, NORMAL_STATE, false);
            IsTransitioning = false;
            if (PreviousContentPresentationSite != null) PreviousContentPresentationSite.Content = null;
        }

        private Storyboard GetStoryboard(string newTransition)
        {
            var presentationGroup = VisualStates.TryGetVisualStateGroup(this, PRESENTATION_GROUP);
            Storyboard newStoryboard = null;
            if (presentationGroup != null)
                newStoryboard = presentationGroup.States
                    .OfType<VisualState>()
                    .Where(state => state.Name == newTransition)
                    .Select(state => state.Storyboard)
                    .FirstOrDefault();
            return newStoryboard;
        }

        private static void OnIsTransitioningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (TransitioningContentControl) d;

            if (!source._allowIsTransitioningWrite)
            {
                source.IsTransitioning = (bool) e.OldValue;
                throw new InvalidOperationException("TransitiotioningContentControl_IsTransitioningReadOnly");
            }
        }

        private static void OnTransitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (TransitioningContentControl) d;
            var oldTransition = e.OldValue as string;
            var newTransition = e.NewValue as string;

            if (source.IsTransitioning) source.AbortTransition();

            // find new transition
            var newStoryboard = source.GetStoryboard(newTransition);

            // unable to find the transition.
            if (newStoryboard == null)
            {
                // could be during initialization of xaml that presentationgroups was not yet defined
                if (VisualStates.TryGetVisualStateGroup(source, PRESENTATION_GROUP) == null)
                {
                    // will delay check
                    source.CurrentTransition = null;
                }
                else
                {
                    // revert to old value
                    source.SetValue(TransitionProperty, oldTransition);

                    throw new ArgumentException(
                        "TransitioningContentControl_TransitionNotFound");
                }
            }
            else
            {
                source.CurrentTransition = newStoryboard;
            }
        }

        private static void OnRestartTransitionOnContentChangePropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ((TransitioningContentControl) d).OnRestartTransitionOnContentChangeChanged((bool) e.OldValue,
                (bool) e.NewValue);
        }

        protected virtual void OnRestartTransitionOnContentChangeChanged(bool oldValue, bool newValue)
        {
        }
    }
}