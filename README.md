![Lyric](https://www.lyricopera.org/globalassets/1920season/don-giovanni/don_giovanni_PDP_1600x1015.jpg) 
# ImageLyric 

　　ImageLyric 软件和它的源代码是免费提供的。遵循来自中国的[木兰宽松许可证v2](http://license.coscl.org.cn/MulanPSL2)。

　　ImageLyric 的研发初心完全是建立在[ImageJ](https://imagej.nih.gov/ij/)的成功应用基础之上的，在这里向[ImageJ](https://imagej.nih.gov/ij/)的作者[Wayne Rasband](https://imagej.net/people/rasband)致以崇高的敬意。在上述基础之上，本软件作者之所以想再造车轮，构建这样一个项目，主要的出发点有几个方面：一、[ImageJ](https://imagej.nih.gov/ij/)在功能界面与人机交互上是有不少可以改进优化的；二、虽然Java语言无所不能，并且有跨平台的优势，但是Java语言在构建桌面应用软件上还是略显笨拙一些；三、作者是一个有20余年技术实践与研发管理的老派工程师，并且工作中也和科研图像、显微图像的处理技术有很多的相关性，所以希望把工作积累的知识与经验充分释放利用起来。

　　ImageLyric 是作者利用业余时间研发的。

　　ImageLyric 当前的定义是一款正在研发的图像处理软件，由[Erian Lu](https://gitee.com/xknife)在2022年春天于北京家中启动研发工作，在第一期产品定义中，为快速验证与实现基本的产品思路与技术路线，ImageLyric将完全参考[ImageJ](https://imagej.nih.gov/ij/)的功能布局与定义，以及实现的功能及效果；因开发语言的差异以及技术架构的不同，所以作者承诺原创，不抄袭代码（也没可能抄袭）。在第一期研发结束后，会在第二期定义新版的功能地图和界面交互。

　　ImageLyric 使用C#语言，在.Net6框架下进行开发，界面采用WPF技术，代码构架基于MVVM思想。第一期仅支持Windows桌面应用。

　　ImageLyric 第1期可能支持的功能将会包括但不限于： **(1)** 支持的图像数据类型：8位、16位、32位数据表达和RGB、aRGB、CMYK、Lab等数据格式。 **(2)** 支持的图像格式：TIFF（未压缩）或原始数据；GIF、JPEG、BMP、PNG、PGM、FITS等常见格式。 **(3)** 图像显示：提供缩放和滚动图像的工具；将一个RGB图像转换为8位索引颜色；对灰度图像应用伪彩色调色板；将一个32位的彩色图像分割成RGB或HSV单独通道进行观察与处理。 **(4)** 选择与选区：创建矩形、椭圆或不规则区域的选择。创建线和点的选择。编辑选择和使用魔杖工具自动创建选择。绘制、填充、清除、过滤或测量选区。保存选区并将其转移到其他图像上。 **(5)** 图像增强：支持8位灰度和RGB彩色图像的平滑、锐化、边缘检测、中值过滤和阈值处理。交互式地调整8、16和32位图像的亮度和对比度。 **(6)** 几何操作：裁剪、缩放、调整大小和旋转；垂直或水平翻转。 **(7)** 分析功能：测量面积，平均值，标准偏差，选择或整个图像的最小和最大值；测量长度和角度。 **(8)** 编辑功能：剪切、复制或粘贴图像或选区；使用AND、OR、XOR或 "混合 "模式粘贴；在图像上添加文本、箭头、矩形、椭圆或多边形。 **(9)** 堆栈能力：在一个窗口中显示一个相关图像的 "堆栈"；将一个文件夹中的图像作为一个堆栈打开；将堆栈保存为多图像的TIFF文件。

　　最后，作者想描述一下初心与发源，其实最主要是看到[ImageJ](https://imagej.nih.gov/ij/)的作者 [Wayne Rasband](https://imagej.net/people/rasband) 的简介后，颇感励志。是的，这就是作者向往的工程师生活中的一个侧写。所以那就开始吧，**但行好事，莫问前程；前路有光，初心莫忘**。

---

![Workbench](https://oss.xknife.net/Workbench_with_a_little_clutter_and_warmth.jpg)