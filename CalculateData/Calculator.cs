using CalculateData.Assets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace CalculateData
{
    public class Calculator
    {
        public List<string> _input;

        public double _B { get; set; }
        public double _H { get; set; }
        public double _t { get; set; }
        public double _Rb { get; set; }
        public double _Eb { get; set; }
        public double _Ry { get; set; }
        public double _Ey { get; set; }
        public double _Ex1 { get; set; }
        public double _Ey1 { get; set; }
        public double _Lo { get; set; }
        public double _dN { get; set; }

        public Calculator(List<string> inputList)
        {
            Logger.Log.Info($"Парсинг введённых значений. Количество значений: {inputList.Count}");
            try
            {
                _B = Convert.ToDouble(inputList[0], CultureInfo.InvariantCulture);
                _H = Convert.ToDouble(inputList[1], CultureInfo.InvariantCulture);
                _t = Convert.ToDouble(inputList[2], CultureInfo.InvariantCulture);
                _Rb = Convert.ToDouble(inputList[3], CultureInfo.InvariantCulture);
                _Eb = Convert.ToDouble(inputList[4], CultureInfo.InvariantCulture);
                _Ry = Convert.ToDouble(inputList[5], CultureInfo.InvariantCulture);
                _Ey = Convert.ToDouble(inputList[6], CultureInfo.InvariantCulture);
                _Ex1 = Convert.ToDouble(inputList[7], CultureInfo.InvariantCulture);
                _Ey1 = Convert.ToDouble(inputList[8], CultureInfo.InvariantCulture);
                _Lo = Convert.ToDouble(inputList[9], CultureInfo.InvariantCulture);
                _dN = Convert.ToDouble(inputList[10], CultureInfo.InvariantCulture);

                Logger.Log.Info(
                    $"Парсинг успешен. B = {_B}, H = {_H}, t = {_t}, Rb = {_Rb}, Eb = {_Eb}, Ry = {_Ry}, Ey = {_Ey}, Ex1 = {_Ex1}, Ey1 = {_Ey1}, Lo = {_Lo}, dN = {_dN}");
            }
            catch (FormatException ex)
            {
                throw new Exception($"Введен неверный тип данных. Данные: {string.Join(", ", inputList)} ");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new Exception($"Введено неверное количество переменных. Данные: {string.Join(", ", inputList)} ");
            }
            catch (Exception ex)
            {
                throw new Exception($"Введённые данные не верны: {ex.Message}. Данные: {string.Join(", ", inputList)} ");
            }
        }

        public double Calculate()
        {
            Logger.Log.Info($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

            var geomCharacteristics = CalculateGeomChar();

            var pointsOnCurve = CalculatePointsOnCurve(geomCharacteristics.Item1.Item1, geomCharacteristics.Item1.Item2, geomCharacteristics.Item2.Item1, geomCharacteristics.Item2.Item2);

            try
            {
                var result = CalculateN(pointsOnCurve);

                Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно. Результат = {result}");

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public double CalculateN((double, double, double, double, double, double) pointsOnCurve)
        {
            Logger.Log.Info($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

            var Nplrd = pointsOnCurve.Item1;
            var Npmrd = pointsOnCurve.Item2;
            var MplRdx = pointsOnCurve.Item3;
            var MplRdy = pointsOnCurve.Item4;
            var Ncrx = pointsOnCurve.Item5;
            var Ncry = pointsOnCurve.Item6;

            Logger.Log.Info($"Nplrd = {Nplrd}, Npmrd = {Npmrd}, MplRdy = {MplRdy}, MplRdx = {MplRdx}, Ncrx = {Ncrx}, Ncry = {Ncry}, dN = {0}");

            var N = new List<double>();

            N.Add(Npmrd);

            var i = 0;
            bool isFirstIter = true;
            while (true)
            {

                if (!isFirstIter)
                {
                    N[i] += _dN;
                }

                Logger.Log.Info($" i = {i}. Первая итерация? {isFirstIter}. Значение силы = {N[i]}");

                var Ned = N[i];

                var kX = 1 / (1 - Ned / Ncrx);
                var kY = 1 / (1 - Ned / Ncry);

                var Medx = Ned * _Ey1 * kX;
                var Medy = Ned * _Ex1 * kY;

                var MplNrdx = GetMplNrd(MplRdx, Ned, Nplrd, Npmrd);
                var MplNrdy = GetMplNrd(MplRdy, Ned, Nplrd, Npmrd);

                var uDx = MplNrdx / MplRdx;
                var uDy = MplNrdy / MplRdy;

                Logger.Log.Info($"Ned = {Ned}, kX = {kX}, kY = {kY}, Medx = {Medx}, Medy = {Medy}, MplNrdx = {MplNrdx}, MplNrdy = {MplNrdy}, uDx = {uDx}, uDy = {uDy}");

                isFirstIter = false;

                if (CalculateCondition(Medy, uDy, MplRdy) < 0.9 && CalculateCondition(Medx, uDx, MplRdx) < 0.9 &&
                   (CalculateCondition(Medy, uDy, MplRdy) + CalculateCondition(Medx, uDx, MplRdx) < 1))
                {
                    Logger.Log.Info($"Одно из условий не выполнено. Условие 1: {CalculateCondition(Medy, uDy, MplRdy)} < 0.9. Условие 2: {CalculateCondition(Medx, uDx, MplRdx)} < 0.9. Условие 3: {(CalculateCondition(Medy, uDy, MplRdy) + CalculateCondition(Medx, uDx, MplRdx))} < 1 ");

                    i = 0;
                    continue;
                }

                i++;
                N.Add(Npmrd);

                if (i >= 5)
                {
                    if (N[i - 1] > Npmrd)
                    {
                        Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно. Результат = {N[i - 1]}");

                        return N[i - 1];
                    }

                    Logger.Log.Error("Слишком гибкий стержень");
                    throw new Exception("Слишком гибкий стержень");
                }

                _dN = _dN / 2;
                N[i] = N[i - 1];
            }
        }

        public double CalculateCondition(double med, double uD, double MplRd)
        {
            var result = med / (uD * MplRd);

            Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно. Результат = {result}");

            return result;
        }

        public double GetMplNrd(double mplRd, double ned, double nplrd, double npmrd)
        {
            Logger.Log.Info($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

            var part1 = ned - nplrd;
            var part2 = mplRd / (npmrd - nplrd);

            var result = Math.Round(part1 * part2, 2, MidpointRounding.ToEven);

            Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно. Результат = {result}");

            return result;
        }

        #region Расчет точек кривой
        public (double, double, double, double, double, double) CalculatePointsOnCurve(double Jxc, double Jxb, double Jyc, double Jyb)
        {
            Logger.Log.Info($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

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

            Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно. Результат: Ncrx = {Ncrx}, Ncry = {Ncry}, NplRd = {NplRd}, NpmRd = {NpmRd}, MplRdx = {MplRdx}, MplRdy = {MplRdy}");

            return (NplRd, NpmRd, MplRdx, MplRdy, Ncrx, Ncry);
        }

        public double GetY()
        {
            Logger.Log.Info($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

            var part1 = _B * _t * 2 * _Ry;
            var part2 = (_H - 2 * _t) * _t * _Rb;

            var part3 = 4 * _t * _Ry;
            var part4 = (_H - 2 * _t) * _Rb;

            var result = (part1 + part2) / (part3 + part4);

            Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно. Результат = {result}");

            return result;
        }

        public double GetX()
        {
            Logger.Log.Info($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

            var part1 = _H * _t * 2 * _Ry;
            var part2 = (_B - 2 * _t) * _t * _Rb;

            var part3 = 4 * _t * _Ry;
            var part4 = (_B - 2 * _t) * _Rb;

            var result = (part1 + part2) / (part3 + part4);
            Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно. Результат = {result}");

            return result;
        }

        public double CalculateMplRd(double x)
        {
            Logger.Log.Info($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

            var part1 = _Rb * (x - _t);
            var part2 = _H - (_t / 2) - (x - _t) / 2;

            var part3 = x * _t * 2 * _Ry;
            var part4 = _H - (_t / 2) - (x / 2);

            var part5 = (_B - 2 * _t) * _t * (_H - _t) * _Ry;

            var part6 = _Ry * (_H - x) * _t * 2;
            var part7 = (_H - x) / 2 - _t / 2;

            var result = Math.Round(part1 * part2 + part3 * part4 + part5 - part6 * part7, 2, MidpointRounding.ToEven);

            Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно. Результат = {result}");

            return result;
        }

        #endregion

        #region Рассчет геом. хар.

        public ((double, double, double), (double, double, double), double) CalculateGeomChar()
        {
            Logger.Log.Info($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

            var Jx = CalculateJx();

            var Jy = CalculateJy();

            var Areol = CalcAreol();

            Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно.");

            return (Jx, Jy, Areol);
        }

        public double CalcAreol()
        {
            Logger.Log.Info($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

            var result = _Eb / _Ey;

            result = result * (_B - 2 * _t);

            result = result * (_H - 2 * _t);

            var part2 = (_H * _t * 2) + ((_B - _t * 2) * _t * 2);

            result = result + part2;

            Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно. Результат = {result}");

            return result;
        }

        public (double, double, double) CalculateJx()
        {
            Logger.Log.Info($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

            var part1 = 2 * ((Math.Pow(_B, 3) * _t) / 12);

            var part2 = (((_H - 2 * _t) * Math.Pow(_t, 3)) / 12) * 2;

            var part3 = (_H - 2 * _t) * Math.Pow(((_B / 2) - (_t / 2)), 2) * _t * 2;

            var part4 = _Eb / _Ey;

            var Jxb = (Math.Pow((_B - 2 * _t), 3) * (_H - 2 * _t)) / 12;

            var Jxc = part1 + part2 + part3;

            var result = (Math.Round(Jxc, 2, MidpointRounding.ToEven), Jxb, Jxc + Jxb * part4);

            Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно. Результат = {result}");

            return result;
        }

        public (double, double, double) CalculateJy()
        {
            Logger.Log.Info($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

            var part1 = 2 * ((Math.Pow(_H, 3) * _t) / 12);

            var part2 = (((_B - 2 * _t) * Math.Pow(_t, 3)) / 12) * 2;

            var part3 = (_B - 2 * _t) * Math.Pow(((_H / 2) - (_t / 2)), 2) * _t * 2;

            var part4 = _Eb / _Ey;

            var Jyb = (Math.Pow((_H - 2 * _t), 3) * (_B - 2 * _t)) / 12;

            var Jyc = part1 + part2 + part3;

            var result = (Math.Round(Jyc, 2, MidpointRounding.ToEven), Jyb, Jyc + Jyb * part4);

            Logger.Log.Info($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно. Результат = {result}");

            return result;
        }

        #endregion

    }
}