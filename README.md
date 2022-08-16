![Lyric](https://www.lyricopera.org/globalassets/1920season/don-giovanni/don_giovanni_PDP_1600x1015.jpg) 
# ImageLyric 

## 序言：但行好事，莫问前程

　　ImageLyric 软件和它的源代码是免费提供的，属于公共领域。不需要许可证。

　　ImageLyric 的研发初心完全是建立在[ImageJ](https://imagej.nih.gov/ij/)的成功基础之上的，作者之所以想构建这样一个项目，主要的出发点有几个方面：一、ImageLyric在功能界面上是有不少可以改进优化的；二、虽然java无所不能，并且有跨平台的优势，但是在构建桌面应用软件上还是略显笨拙一些；三、作者是一个老工程师，工作也和科研图像、显微图像处理相关，所以希望把工作积累的经验充分释放利用起来。ImageLyric是作者利用业余时间研发的。

　　ImageLad 是一款正在研发的公共的图像处理软件，由[Erian Lu](https://gitee.com/xknife)在2022年春天于北京家中启动研发工作，在第一期中，ImageLyric完全参考[ImageJ](https://imagej.nih.gov/ij/)的功能布局，以及实现的功能，基本可以定义就是“抄袭”。在第一期研发结束后，会在第二期定义新版的功能地图和界面交互。

　　ImageLyric 将会是一个开放架构的软件，支持用户自定义插件和宏。使用宏来实现任务的自动化和创建自定义工具。使用命令记录器生成宏代码，并使用宏调试器对其进行调试。

　　ImageLyric 第1期可能支持的功能将会包括但不限于： **(1)** 支持的图像数据类型：8位灰度或索引颜色，16位无符号整数，32位浮点和RGB颜色。 **(2)** 支持的图像格式：打开并保存所有支持的数据类型为TIFF（未压缩）或原始数据。打开并保存GIF、JPEG、BMP、PNG、PGM、FITS和ASCII。打开DICOM。使用URL打开TIFFs, GIFs, JPEGs, DICOMs和原始数据。使用插件打开和保存许多其他格式。 **(3)** 图像显示：提供缩放（1:32至32:1）和滚动图像的工具。所有的分析和处理功能都能在任何放大系数下工作。 **(4)** 选择与选区：创建矩形、椭圆或不规则区域的选择。创建线和点的选择。编辑选择和使用魔杖工具自动创建选择。绘制、填充、清除、过滤或测量选区。保存选区并将其转移到其他图像上。 **(5)** 图像增强：支持8位灰度和RGB彩色图像的平滑、锐化、边缘检测、中值过滤和阈值处理。交互式地调整8、16和32位图像的亮度和对比度。 **(6)** 几何操作：裁剪、缩放、调整大小和旋转。垂直或水平翻转。 **(7)** 分析功能：测量面积，平均值，标准偏差，选择或整个图像的最小和最大值。测量长度和角度。使用现实世界的测量单位，如毫米。使用密度标准进行校准。生成柱状图和剖面图。 **(8)** 编辑功能：剪切、复制或粘贴图像或选区。使用AND、OR、XOR或 "混合 "模式粘贴。在图像上添加文本、箭头、矩形、椭圆或多边形。 **(9)** 颜色处理：将一个32位的彩色图像分割成RGB或HSV组件。将8位分量合并成一个彩色图像。将一个RGB图像转换为8位索引颜色。对灰度图像应用伪彩色调色板。 **(10)** 堆栈能力：在一个窗口中显示一个相关图像的 "堆栈"。用一个单一的命令处理整个堆栈。将一个文件夹中的图像作为一个堆栈打开。将堆栈保存为多图像的TIFF文件。

　　当作者使用了ImageJ这款已经在科研领域广泛流传的软件，并看到其作者 [Wayne Rasband](https://imagej.net/people/rasband) 的简介后，颇感励志。这就是作者向往的生活中的一部份。所以那就开始吧，但行好事，莫问前程；前路有光，初心莫忘。

---

![Workbench](https://oss.xknife.net/Workbench_with_a_little_clutter_and_warmth.jpg)