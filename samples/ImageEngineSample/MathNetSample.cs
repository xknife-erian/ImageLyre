using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace ImageEngineSample;

public class MathNetSample
{
    public MathNetSample()
    {
        //DenseMatrix:一个具有密集存储的矩阵类。底层存储是一个一维数组，以列为主的顺序（逐列）。
        //矩阵的定义和初始化
        var matrix1 = new DenseMatrix(3); //3维方阵
        var matrix2 = new DenseMatrix(2, 2); //2维方阵
        var matrix3 = new DenseMatrix(2, 3); //2×3矩阵
        var matrix4 = new DenseMatrix(3, 2); //3×2矩阵
        var matrix5 = DenseMatrix.CreateIdentity(3); //单位矩阵
        Complex[,] x =
        {
            {1.0, 2.0, 3.0},
            {3.0, 4.0, 5.0},
            {3.0, 6.0, 5.0}
        };
        var matrix6 = DenseMatrix.OfArray(x);

        var submatrix = matrix6.SubMatrix(1, 2, 1, 2); //取从第1行开始的2行，第1列开始的2列 子矩阵
        var row = matrix6.Row(1, 1, 2); //取从第1行第1列开始的2个行元素 
        var column = matrix6.Column(1, 1, 2); //取从第1列第1行开始的2个列元素      
        var data1 = matrix6.ToRowArrays(); //矩阵变为行向量
        var data2 = matrix6.ToColumnArrays(); //矩阵变为列向量
        var data3 = matrix6.Diagonal(); //取矩阵的对角线元素向量
    }
}