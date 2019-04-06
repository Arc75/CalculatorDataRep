using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CalculateData
{
    public class Calculator
    {
        public List<string> _input;

        private double _B { get; set; }
        private double _H { get; set; }
        private double _t { get; set; }
        private double _Rb { get; set; }
        private double _Eb { get; set; }
        private double _Ry { get; set; }
        private double _Ey { get; set; }
        private double _Ex1 { get; set; }
        private double _Ey1 { get; set; }
        private double _Lo { get; set; }
        private double _dN { get; set; }

        public Calculator(List<string> inputList)
        {
            try
            {
                _B = Convert.ToDouble(inputList[0]);
                _H = Convert.ToDouble(inputList[1]);
                _t = Convert.ToDouble(inputList[2]);
                _Rb = Convert.ToDouble(inputList[3]);
                _Eb = Convert.ToDouble(inputList[4]);
                _Ry = Convert.ToDouble(inputList[5]);
                _Ey = Convert.ToDouble(inputList[6]);
                _Ex1 = Convert.ToDouble(inputList[7]);
                _Ey1 = Convert.ToDouble(inputList[8]);
                _Lo = Convert.ToDouble(inputList[9]);
                _dN = Convert.ToDouble(inputList[10]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Введённые данные не верны: {ex.Message}. Данные: {string.Join(", ", inputList)} ");
            }
        }

        public double Calculate()
        {
            var geomCharacteristics = CalculateGeomChar();

            var pointsOnCurve = CalculatePointsOnCurve(geomCharacteristics.Item1.Item1, geomCharacteristics.Item1.Item2, geomCharacteristics.Item2.Item1, geomCharacteristics.Item2.Item2);

            return CalculateN(pointsOnCurve);
        }

        public double CalculateN((double, double, double, double, double, double) pointsOnCurve)
        {
            var Nplrd = pointsOnCurve.Item1;
            var Npmrd = pointsOnCurve.Item2;
            var MplRdx = pointsOnCurve.Item3;
            var MplRdy = pointsOnCurve.Item4;
            var Ncrx = pointsOnCurve.Item5;
            var Ncry = pointsOnCurve.Item6;
            var dN = 0;

            var N = new List<double>();

            N.Add(Npmrd);

            var i = 0;
            bool isFirstIter = true;
            while (true)
            {
                if (!isFirstIter)
                {
                    N[i] += dN;
                }

                var Ned = N[i];

                var kX = 1 / (1 - Ned / Ncrx);
                var kY = 1 / (1 - Ned / Ncry);

                var Medx = Ned * _Ey1 * kX;
                var Medy = Ned * _Ex1 * kY;

                var MplNrdx = GetMplNrd(MplRdx, Ned, Nplrd, Npmrd);
                var MplNrdy = GetMplNrd(MplRdy, Ned, Nplrd, Npmrd);

                var uDx = MplNrdx / MplRdx;
                var uDy = MplNrdy / MplRdy;

                if (CalculateCondition(Medy, uDy, MplRdy) < 0.9 && CalculateCondition(Medx, uDx, MplRdx) < 0.9 &&
                    (CalculateCondition(Medy, uDy, MplRdy) + CalculateCondition(Medx, uDx, MplRdx) < 1))
                {
                    i = 0;
                    isFirstIter = false;
                    continue;
                }
                else
                {

                }
            }

            return 1;
        }

        private double CalculateCondition(double med, double uD, double MplRd)
        {
            return med / (uD * MplRd);
        }

        private double GetMplNrd(double mplRd, double ned, double nplrd, double npmrd)
        {
            var part1 = ned - nplrd;
            var part2 = mplRd / (npmrd - nplrd);

            return Math.Round(part1 * part2, 2, MidpointRounding.ToEven);
        }

        #region Расчет точек кривой
        public (double, double, double, double, double, double) CalculatePointsOnCurve(double Jxc, double Jxb, double Jyc, double Jyb)
        {
            var ELxeff = ((_Ey * Jxc) + (0.5 * _Eb * Jxb)) * 0.9;
            var ELyeff = ((_Ey * Jyc) + (0.5 * _Eb * Jyb)) * 0.9;
            var Ncrx = (Math.Pow(Math.PI, 2) * ELxeff) / Math.Pow(_Lo, 2);
            var Ncry = (Math.Pow(Math.PI, 2) * ELyeff) / Math.Pow(_Lo, 2);

            var Ab = (_B - 2 * _t) * (_H - 2 * _t);
            var Ay = _H * _t + (_B - 2 * _t) * _t;

            var NplRd = (_Rb * Ab) + (_Ry * Ay);
            var NpmRd = _Rb * Ab;
            
            var MplRdx = CalculateMplRd(GetX());
            var MplRdy = CalculateMplRd(GetY());

            return (NplRd, NpmRd, MplRdx, MplRdy, Ncrx, Ncry);
        }

        public double GetY()
        {
            var part1 = _B * _t * 2 * _Ry;
            var part2 = (_H - 2 * _t) * _t * _Rb;

            var part3 = 4 * _t * _Ry;
            var part4 = (_H - 2 * _t) * _Rb;

            return (part1 + part2) / (part3 + part4);
        }

        public double GetX()
        {
            var part1 = _H * _t * 2 * _Ry;
            var part2 = (_B - 2 * _t) * _t * _Rb;

            var part3 = 4 * _t * _Ry;
            var part4 = (_B - 2 * _t) * _Rb;

            return (part1 + part2) / (part3 + part4);
        }

        public double CalculateMplRd(double x)
        {
            var part1 = _Rb * (x - _t);
            var part2 = _H - (_t / 2) - (x - _t) / 2;

            var part3 = x * _t * 2 * _Ry;
            var part4 = _H - (_t / 2) - (x / 2);

            var part5 = (_B - 2 * _t) * _t * (_H - _t) * _Ry;

            var part6 = _Ry * (_H - x) * _t * 2;
            var part7 = (_H - x) / 2 - _t / 2;

            return Math.Round(part1 * part2 + part3 * part4 + part5 - part6 * part7, 2, MidpointRounding.ToEven);
        }

        #endregion

        #region Рассчет геом. хар.

        public ((double, double, double), (double, double, double), double) CalculateGeomChar()
        {
            var Jx = CalculateJx();

            var Jy = CalculateJy();

            var Areol = CalcAreol();

            return (Jx, Jy, Areol);
        }

        public double CalcAreol()
        {
            var result = _Eb / _Ey;

            result = result * (_B - 2 * _t);

            result = result * (_H - 2 * _t);

            var part2 = (_H * _t * 2) + ((_B - _t * 2) * _t * 2);

            return result + part2;
        }

        public (double, double, double) CalculateJx()
        {
            var part1 = 2 * ((Math.Pow(_B, 3) * _t) / 12);

            var part2 = (((_H - 2 * _t) * Math.Pow(_t, 3)) / 12) * 2;

            var part3 = (_H - 2 * _t) * Math.Pow(((_B / 2) - (_t / 2)), 2) * _t * 2;

            var part4 = _Eb / _Ey;

            var Jxb = (Math.Pow((_B - 2 * _t), 3) * (_H - 2 * _t)) / 12;

            var Jxc = part1 + part2 + part3;

            return (Math.Round(Jxc, 2, MidpointRounding.ToEven), Jxb, Jxc + Jxb * part4);
        }

        public (double, double, double) CalculateJy()
        {
            var part1 = 2 * ((Math.Pow(_H, 3) * _t) / 12);

            var part2 = (((_B - 2 * _t) * Math.Pow(_t, 3)) / 12) * 2;

            var part3 = (_B - 2 * _t) * Math.Pow(((_H / 2) - (_t / 2)), 2) * _t * 2;

            var part4 = _Eb / _Ey;

            var Jyb = (Math.Pow((_H - 2 * _t), 3) * (_B - 2 * _t)) / 12;

            var Jyc = part1 + part2 + part3;

            return (Math.Round(Jyc, 2, MidpointRounding.ToEven), Jyb, Jyc + Jyb * part4);
        }

        #endregion

    }
}
