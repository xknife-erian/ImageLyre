![Lad](https://www.kidsgen.com/short_stories/images/lenny.png)
# ImageLad

## 序言：但行好事，莫问前程

　　ImageLad 软件和它的源代码是免费提供的，属于公共领域。不需要许可证。

　　ImageLad 的研发初心完全是建立在[ImageJ](https://imagej.nih.gov/ij/)的成功基础之上的，作者之所以想构建这样一个项目，主要的出发点有几个方面：一、ImageJ在功能界面上是有不少可以改进优化的；二、虽然java无所不能，并且有跨平台的优势，但是在构建桌面应用软件上还是略显笨拙一些；三、作者是一个老工程师，工作也和科研图像、显微图像处理相关，所以希望把工作积累的经验充分释放利用起来。ImageLad是作者利用业余时间研发的。

　　ImageLad 是一款正在研发的公共的图像处理软件，由[Erian Lu](https://gitee.com/xknife)在2022年春天于北京家中启动研发工作，在第一期中，ImageLad完全参考[ImageJ](https://imagej.nih.gov/ij/)的功能布局，以及实现的功能，基本可以定义就是“抄袭”。在第一期研发结束后，会在第二期定义新版的功能地图和界面交互。

　　ImageLad 将会是一个开放架构的软件，支持用户自定义插件和宏。使用宏来实现任务的自动化和创建自定义工具。使用命令记录器生成宏代码，并使用宏调试器对其进行调试。

　　ImageLad 第1期可能支持的功能将会包括但不限于： **(1)** 支持的图像数据类型：8位灰度或索引颜色，16位无符号整数，32位浮点和RGB颜色。 **(2)** 支持的图像格式：打开并保存所有支持的数据类型为TIFF（未压缩）或原始数据。打开并保存GIF、JPEG、BMP、PNG、PGM、FITS和ASCII。打开DICOM。使用URL打开TIFFs, GIFs, JPEGs, DICOMs和原始数据。使用插件打开和保存许多其他格式。 **(3)** 图像显示：提供缩放（1:32至32:1）和滚动图像的工具。所有的分析和处理功能都能在任何放大系数下工作。 **(4)** 选择与选区：创建矩形、椭圆或不规则区域的选择。创建线和点的选择。编辑选择和使用魔杖工具自动创建选择。绘制、填充、清除、过滤或测量选区。保存选区并将其转移到其他图像上。 **(5)** 图像增强：支持8位灰度和RGB彩色图像的平滑、锐化、边缘检测、中值过滤和阈值处理。交互式地调整8、16和32位图像的亮度和对比度。 **(6)** 几何操作：裁剪、缩放、调整大小和旋转。垂直或水平翻转。 **(7)** 分析功能：测量面积，平均值，标准偏差，选择或整个图像的最小和最大值。测量长度和角度。使用现实世界的测量单位，如毫米。使用密度标准进行校准。生成柱状图和剖面图。 **(8)** 编辑功能：剪切、复制或粘贴图像或选区。使用AND、OR、XOR或 "混合 "模式粘贴。在图像上添加文本、箭头、矩形、椭圆或多边形。 **(9)** 颜色处理：将一个32位的彩色图像分割成RGB或HSV组件。将8位分量合并成一个彩色图像。将一个RGB图像转换为8位索引颜色。对灰度图像应用伪彩色调色板。 **(10)** 堆栈能力：在一个窗口中显示一个相关图像的 "堆栈"。用一个单一的命令处理整个堆栈。将一个文件夹中的图像作为一个堆栈打开。将堆栈保存为多图像的TIFF文件。

　　当作者使用了ImageJ这款已经在科研领域广泛流传的软件，并看到其作者 [Wayne Rasband](https://imagej.net/people/rasband) 的简介后，颇感励志。这就是作者向往的生活中的一部份。所以那就开始吧，但行好事，莫问前程；前路有光，初心莫忘。

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