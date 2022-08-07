using System;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OpenCvSharp;

namespace WPFSample.Panes;

public class CamSampleViewModel : ObservableRecipient
{
    private readonly VideoCapture _cap = new();

    public ICommand Func1 => new RelayCommand(CamFunc1);
    public ICommand Func2 => new RelayCommand(CamFunc2);

    /// <summary>
    ///     固定背景对比,获取第一帧背景，启动后，摄像头的位置不能动
    /// </summary>
    private void CamFunc1()
    {
        var bgMat = new Mat();

        var isFirstFrame = true; //判断是不是第一帧

        _cap.Open(0); //打开摄像头
        if (!_cap.IsOpened())
            return; //判断摄像头是否已经打开

        while (true)
        {
            var capMat = _cap.RetrieveMat();
            if (capMat.Empty())
                continue;
            //   Cv2.CvtColor(capMat, capMat, ColorConversionCodes.BGR2GRAY);   //转成灰度图
            if (isFirstFrame)
            {
                isFirstFrame = false;
                bgMat = capMat;
            }

            for (var i = 0; i < capMat.Rows; i += 10)
            for (var j = 0; j < capMat.Cols; j += 10)
                //这样写不知道有没有什么问题，但是能实现效果，先就这样写,获取像素点的rgb值做对比，其中50是差值，可根据自己需求来调节
                if (Math.Abs(capMat.Get<Vec3b>(i, j).Item0 - bgMat.Get<Vec3b>(i, j).Item0) > 50 ||
                    Math.Abs(capMat.Get<Vec3b>(i, j).Item1 - bgMat.Get<Vec3b>(i, j).Item1) > 50 ||
                    Math.Abs(capMat.Get<Vec3b>(i, j).Item2 - bgMat.Get<Vec3b>(i, j).Item2) > 50)
                    Cv2.Circle(capMat, new Point(j, i), 2, Scalar.White, 4); //画圆，在capMat的(j,i)位置画半径为2，厚度为4的白色的实心圆

            Cv2.ImShow("CamFunc1", capMat);
            Cv2.WaitKey(20);
        }
    }

    /// <summary>
    ///     前后两帧对比，此时物体需要一直移动才能被检测出
    /// </summary>
    private void CamFunc2()
    {
        var lastMat = new Mat();

        var isEvenFrame = true; //判断是不是偶数帧，即为参照帧    

        _cap.Open(0); //打开摄像头
        if (!_cap.IsOpened())
            return; //判断摄像头是否已经打开

        while (true)
        {
            var capMat = _cap.RetrieveMat();
            if (capMat.Empty())
                continue;
            //   Cv2.CvtColor(capMat, capMat, ColorConversionCodes.BGR2GRAY);   //转成灰度图
            if (isEvenFrame)
            {
                isEvenFrame = false;
                lastMat = capMat;
            }

            else
            {
                for (var i = 0; i < capMat.Rows; i += 10) //遍历capMat所有像素
                for (var j = 0; j < capMat.Cols; j += 10)
                    if (Math.Abs(capMat.Get<Vec3b>(i, j).Item0 - lastMat.Get<Vec3b>(i, j).Item0) > 50 ||
                        Math.Abs(capMat.Get<Vec3b>(i, j).Item1 - lastMat.Get<Vec3b>(i, j).Item1) > 50 ||
                        Math.Abs(capMat.Get<Vec3b>(i, j).Item2 - lastMat.Get<Vec3b>(i, j).Item2) > 50)
                        Cv2.Circle(capMat, new Point(j, i), 2, Scalar.White, 4);
                isEvenFrame = true;
            }

            Cv2.ImShow("CamFunc2", capMat);
            Cv2.WaitKey(20);
        }
    }
}