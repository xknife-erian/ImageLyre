<h1 align="center">
NKnife.ImageLaka
</h1>

---
## 软件技术框架设计

- [ ] 插件服务
- [x] 命令行服务
- [x] 宏动作管理服务
- [ ] C语言库调用服务
- [ ] 多语言切换服务
- [ ] 快捷键管理服务
- [x] 多图切换服务
- [ ] 选项与习惯服务
- [x] 多窗体管理服务

---
## 软件核心功能(图像处理基本功能）

#### 第1阶段：
- 图像预处理1：打开常见类型图片，提取图片内部信息；保存图像；
- 图像预处理2：缩放、裁剪、图像模式与通道变化；
- 图像绘制1：描边、画线、绘制文字、绘制形状、形状填充、框选；
- 图像绘制2：曲线路径、选区的边缘羽化和消除锯齿；
- 图低绘制3：条件选区、条件绘制、条件填充、条件计数；
- 图像变换：主要为图像旋转、几何变换、仿射变换和透视变换；
- 基本处理1：亮度/对比度、色阶、曲线、曝光度、饱和度；
- 基本处理2：颜色通道显示、分离与合并通道；
- 灰度处理：显示灰度直方图、灰度均衡、色调调整；
- 锐化与模糊：表面模糊、高斯模糊、梯度锐化、Laplace锐化等；
- 噪声处理：减噪；椒盐噪声、高斯噪声等；

#### 第2阶段：
- 边缘检测：其中包括Roberts算子、Sobel算子、Laplace算子、Prewitt算子、Canny算子、Krisch算子等；
- 滤波处理：包括均值滤波、中值滤波、边窗滤波、形态学滤波、高斯滤波等；
- 背景处理：实现阈值分割、OSTU和Kittler静态阈值分割、帧间差分和高斯混合背景等；
- 特征明显操作：包括LBP、直方图检测、模板匹配、颜色匹配、Gabor滤波等；
- 特征提取：实现SIFT算法、ORB算法、坐标点SVM、Haar算法等。
- 摄像标定：实现摄像机标定(立体匹配)等；

---
## 研发记录

#### **2022/3/11**
- [ ] 主窗体显示位置
- [ ] 图片容器窗体显示位置及当较大时缩小显示
- [ ] 图像基本信息提取
- [ ] 图像直方图，以及通道直方图，仿photoshop界面

#### **2022/3/10**
- [x] RGB图向8bit图转换
- [ ] 主窗体显示位置
- [ ] 图片容器窗体显示位置及当较大时缩小显示
- [x] 图像基本处理菜单排布

#### **2022/3/9**
以下均未完成，需继续
- [ ] RGB图向8bit图转换
- [ ] 主窗体显示位置
- [ ] 图片容器窗体显示位置及当较大时缩小显示
- [ ] 图像基本处理菜单排布

#### **2022/3/8**
- [x] 图片容器窗体完成雏形
##### Next
- [ ] 主窗体显示位置
- [ ] 图片容器窗体显示位置及当较大时缩小显示
- [ ] 图像基本处理菜单排布