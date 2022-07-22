using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using WPFSample.Sub;

namespace WPFSample
{
    public class Modules : Module
    {
        #region Overrides of Module

        /// <summary>Override to add registrations to the container.</summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<Workbench>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<WorkbenchViewModel>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<HistogramSampleViewModel>().AsImplementedInterfaces().AsSelf().SingleInstance();
        }

        #endregion
    }
}
