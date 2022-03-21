![Lad](https://www.kidsgen.com/short_stories/images/lenny.png)
# ImageLad

> 但行好事，莫问前程。

> 作者承诺本软件使用完全免费，并全部开源。

---

　　ImageLad是一个正在研发的公共的图像处理软件，由Erian Lu在2022年春天启动研发工作，在当前的研发进展的过程中，完全参考[ImageJ](https://imagej.nih.gov/ij/)的功能布局，一是向经典致敬，二是让先期的一些用户更容易接受和迁移。在基本功能开发的第一期几个阶段结束后，会定义新版的功能交互布局。

　　ImageLad是一个开放结构的软件，支持用户自定义插件和宏。

　　ImageLad能够显示，编辑，分析，处理，保存，打印8位，16位，32位的图片，支持TIFF, PNG, GIF, JPEG, BMP, DICOM, FITS等多种格式。ImageLad支持图像栈功能，即在一个窗口里以多线程的形式层叠多个图像并行处理。只要内存允许，ImageLad能打开任意多的图像进行处理。除了基本的图像操作，比如缩放，旋转，扭曲，平滑处理外，ImageLad还能进行图片的区域和像素统计，间距，角度计算，能创建柱状图和剖面图，进行傅里叶变换。

## 第1期软件的核心功能(研发进行中)

### 第1期第1阶段：

- 图像预处理1：打开常见类型图片[∨]，提取图片内部信息；保存图像；
- 图像预处理2：缩放、裁剪、图像模式转换[∨]、通道转换；
- 图像绘制1：画线、绘制文字、绘制形状、形状填充、框选；
- 图像绘制2：ROI功能；
- 图像绘制3：曲线路径、选区的边缘羽化和消除锯齿；
- 图像绘制4：条件选区、条件绘制、条件填充、条件计数；
- 图像变换：主要为图像旋转、几何变换、仿射变换和透视变换；
- 基本处理1：亮度/对比度、色阶、曲线、曝光度、饱和度；
- 基本处理2：颜色通道显示、分离与合并通道；
- 灰度处理：显示灰度直方图[∨]、灰度均衡、色调调整；
- 锐化与模糊：表面模糊、高斯模糊、梯度锐化、Laplace锐化等；
- 噪声处理：减噪；椒盐噪声、高斯噪声等；

### 第1期第2阶段：
- 边缘检测：其中包括Roberts算子、Sobel算子、Laplace算子、Prewitt算子、Canny算子、Krisch算子等；
- 滤波处理：包括均值滤波、中值滤波、边窗滤波、形态学滤波、高斯滤波等；
- 背景处理：实现阈值分割、OSTU和Kittler静态阈值分割、帧间差分和高斯混合背景等；
- 特征明显操作：包括LBP、直方图检测、模板匹配、颜色匹配、Gabor滤波等；
- 特征提取：实现SIFT算法、ORB算法、坐标点SVM、Haar算法等。
- 摄像标定：实现摄像机标定(立体匹配)等；