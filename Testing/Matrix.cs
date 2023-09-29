using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreRegression
{
    public class Matrix
    {
        public List<List<double>> data;

        public Matrix(List<List<double>> data)
        {
            this.data = new List<List<double>>(data.Count);

            double[] tempLine = new double[data[0].Count];

            for (int i = 0; i < data.Count; i++)
            {
                data[i].CopyTo(tempLine);

                this.data.Add(tempLine.ToList());
            }
        }

        public void Add(List<double> newLine)
        {
            double[] temp = new double[data[0].Count];

            newLine.CopyTo(temp);
            this.data.Add(newLine.ToList());
        }

        public Matrix Transpose()
        {
            List<List<double>> result = new List<List<double>>();

            for (int i = 0; i < this.data[0].Count; i++)
            {
                List<double> add = new List<double>();

                for (int j = 0; j < this.data.Count; j++)
                {
                    add.Add(this.data[j][i]);
                }

                result.Add(add);
            }

            return new Matrix(result);
        }



        public Matrix Multiplicate(Matrix right)
        {
            List<List<double>> result = new List<List<double>>();

            for (int i = 0; i < this.data.Count; i++)
            {
                List<double> add = new List<double>();
                for (int j = 0; j < right.data[0].Count; j++)
                {
                    double res = 0;
                    for (int k = 0; k < right.data.Count; k++)
                    {
                        res += this.data[i][k] * right.data[k][j];
                    }
                    add.Add(res);
                }
                result.Add(add);
            }

            return new Matrix(result);
        }

        public Matrix Multiplicate(double right)
        {
            List<List<double>> result = new List<List<double>>();

            for (int i = 0; i < this.data.Count; i++)
            {
                List<double> add = new List<double>();
                for (int j = 0; j < this.data[0].Count; j++)
                {
                    add.Add(this.data[0][j] * right);
                }
                result.Add(add);
            }

            return new Matrix(result);
        }

        public Matrix Minor(int i_exc, int j_exc)
        {
            List<List<double>> result = new List<List<double>>();
            for (int i = 0; i < this.data.Count; i++)
            {
                if (i != i_exc)
                {
                    List<double> temp = new List<double>();
                    for (int j = 0; j < this.data[0].Count; j++)
                        if (j != j_exc) temp.Add(this.data[i][j]);

                    result.Add(temp);
                }
            }

            return new Matrix(result);
        }

        public double Determinant()
        {
            if (this.data.Count == 1)
                return this.data[0][0];

            double det = 0;

            for (int i = 0; i < this.data.Count; i++)
            {
                double mn = (i % 2 == 0) ? 1 : -1;

                det += mn * this.Minor(0, i).Determinant() * this.data[0][i];
            }

            return det;
        }

        public Matrix Reverce()
        {
            List<List<double>> result = new List<List<double>>();

            if (this.data.Count == 1)
                return new Matrix(new List<List<double>> { new List<double> { 1 / this.data[0][0] } });

            double det = this.Determinant();

            for (int i = 0; i < this.data.Count; i++)
            {
                result.Add(new List<double>());

                for (int j = 0; j < this.data.Count; j++)
                {
                    Matrix temp = this.Minor(i, j);
                    result[i].Add((((i + j) % 2 == 0) ? 1 : -1) / det * temp.Determinant());
                }
            }

            return (new Matrix(result));
        }

        List<double> sum(List<double> left, List<double> right)
        {
            List<double> result = new List<double>();
            for (int i = 0; i < left.Count; i++)
                result.Add(left[i] + right[i]);

            return result;
        }

        List<double> mul(List<double> left, double right)
        {
            List<double> result = new List<double>();
            for (int i = 0; i < left.Count; i++)
                result.Add(left[i] * right);

            return result;
        }
    }
}
