using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App
{
    class flashtest
    {
        List<double[]> inputs;
        public flashtest(List<double[]>Inputs)
        {
            inputs = Inputs;
            pressurecolumn = 0;
            volumecolumn = 1;
            limit = 0.25;
            inputsline1 = new List<double[]>();
            inputsline2 = new List<double[]>();
            relvols = new List<double>();
            yfunc = new List<double>();
            inputslineyfunc = new List<double[]>();
        }
        public int pressurecolumn;
        public int volumecolumn;
        public line line1;
        public line line2;
        public line yfline;
        public  List<double[]> inputsline1;
        public List<double[]> inputsline2;
        public List<double[]> inputslineyfunc;
        public double[] intercept;
        public List<double> relvols;
        public List<double> yfunc;
        public double co;
        public string report;
        double limit;
        public void solve()
        {
            line1 = new line();
            line2 = new line();
            yfline = new line();
            var dt = 0.0;
            var lastg = inputs[0];
            int switchh = 0 ;
            int countpoints = 4;

            for(int i = 1; i < inputs.Count; i++)
            {
                var odt = (lastg[pressurecolumn] - inputs[i][pressurecolumn]) / (lastg[volumecolumn] - inputs[i][volumecolumn]);
                if (dt == 0)
                {
                     dt=odt;
                    inputsline1.Add(lastg);
                }
                var chg = (dt - odt)/dt;
                dt = odt;
                if (Math.Abs( chg) > limit)
                {
                    switchh = 1;
                }
                if (switchh == 0)
                {
                    inputsline1.Add(inputs[i]);
                }
                else
                {
                    if (countpoints > 0)
                    {
                        inputsline2.Add(inputs[i]);
                    }
                    else
                    {
                        break;
                    }
                    countpoints--;
                }
                lastg = inputs[i];
            }

           // line1.fit(inputsline1, pressurecolumn, volumecolumn,maxsteps:200);
            line1.fit2(inputsline1, pressurecolumn, volumecolumn);
            line2.fit2(inputsline2, pressurecolumn, volumecolumn);
            intercept = line.getintersiction(line1, line2);
            var relvol = intercept[0];
            //tfunc
            foreach(var t in inputs)
            {
                relvols.Add(t[volumecolumn] / relvol);
                var rel = t[volumecolumn] / relvol;
                var yf = (intercept[1] - t[pressurecolumn]) / (t[pressurecolumn] * (rel - 1));
                yfunc.Add(0);
            }
            for(int i = inputsline1.Count; i < inputs.Count; i++)
            {
                var rel = relvols[i];
                var yf = (intercept[1] - inputs[i][pressurecolumn]) / (inputs[i][pressurecolumn] * (rel - 1));
                yfunc[i]=yf;
            }
            for( int i = inputs.Count-1; i >3+inputsline1.Count; i--)
            {
                double[] r = { inputs[i][pressurecolumn],yfunc[i] };
                inputslineyfunc.Add(r);
            }
            yfline.fit2(inputslineyfunc, yindx: 1, xindex: 0);

            for (int i = 0; i <  inputsline1.Count-1; i++)
            {
                co = (-1 / relvols[i + 1]) * ((relvols[i] - relvols[i + 1]) / (inputsline1[i][pressurecolumn] - inputsline1[i + 1][pressurecolumn]));
                co = Math.Abs(co);
                break;
            }
            report += "Co = " + co;
        }
       
    }
    class diffrentialtest
    {
        public List<double[]> inputs;
        public List<double> bg;
        public List<double> z;
        public List<double> Rsd;
        public List<double> BOD;
        public List<double> BtD;
        int pi;
        int gi1;
        int gi2;
        int oi;
        int ig;
        double T;//'f
        double Vosc;
        double totalgas;
        double Rsdb;
        double tsc;
        public diffrentialtest(List<double[]> Inputs,int Pi=0,int Gi1=1, int Gi2=2,int Oi=3 , int Ig=4,double t=220,double vosc=39.572)
        {
            pi = Pi;
            gi1 = Gi1;
            gi2 = Gi2;
            oi = Oi;
            ig = Ig;
            inputs = Inputs;
            Vosc = vosc;// * 0.000035314666572222;
            T = t+ 459.7;
            tsc = 60+ 459.7;
            foreach(var l in inputs)
            {
                totalgas += l[gi2];
                
            }
            foreach(var l in inputs)
            {
                if (l[gi1] == 0)
                {
                    Rsdb = (totalgas) / (Vosc *6.29 / 1000000);
                }
            }
            
        }
        public void solve()
        {
            //0.000035314666572222
            //Calculating bg,z
            bg = new List<double>();
            z = new List<double>();
            Rsd = new List<double>();
            BOD = new List<double>();
            BtD = new List<double>();
            var tg = 0.0;
            foreach(var l in inputs)
            {
                var bgg = l[gi1] * 0.000035314666572222 / l[gi2];
                bg.Add(bgg);//(VG)p,t/(VG)s.c.
                z.Add(l[gi1] *(l[pi]+14.65)*tsc*(35.315/1000000)/(T*l[gi2]*14.65)) ;//bg*p/(0.02827*T)
                tg += l[gi2];
                var rsd = (totalgas - tg) / (Vosc*6.29/1000000);
                Rsd.Add(rsd);
                var bod = l[oi] / (Vosc);
                BOD.Add(bod);
                BtD.Add(bod+(Rsdb-rsd)*bgg/(5.615));
            }
        }
    }
    public class sepratortest
    {
        public double[] VS;
        public string report;
        public sepratortest(decimal[]vs)
        {
            VS = new double[vs.Length];
            for(int i = 0; i < vs.Length; i++)
            {
                VS[i] = Convert.ToDouble(vs[i]);
            }
        }
        public void solve()
        {
            report += "Separator GOR = "+VS[4]/(VS[1]*6.29/1000000)+" Scf/SP bbl";
            report += "\r\n Stock tank GOR = "+VS[5]/(VS[2]*6.29/1000000)+" Scf/St bbl";
            report += "\r\n Stock tank GOR @ SC = "+VS[5]/(VS[3]*6.29/1000000)+" Scf/St bbl";
            report += "\r\n Separator GOR @ SC = "+VS[4]/(VS[3]*6.29/1000000)+" Scf/St bbl";
            report += "\r\n Bosb = "+VS[0]/(VS[3])+" res bbl/stb";
            report += "\r\n Separator shrinkage factor = "+VS[1]/(VS[3])+" SP bbl/stb";
            report += "\r\n Stock tank shrinkage factor = "+VS[2]/(VS[3])+" St bbl/stb";
            report += "\r\n API = "+(141.5/VS[6]-131.5)+" St bbl/stb";

        }
    }
    public class line
    {
        double slope;
        double a;//y=a+bx;
        public void fit(List<double[]>inputs,int yindx=0,int xindex = 1,int maxsteps=1000,double stopacc=100)
        {
            double[] sides = { 100000 ,-100000};
            double[] sides2 = { 100000 ,-100000};
            
         //   int st = 0;
            for(int st = 0; st < maxsteps; st++)
            {
                var bestacc = -100000000.0;
                var indexbest = 0;
                var i = 0;
                var acc = 0.0;
                if (st > 0.5 * maxsteps)
                {

                }
                foreach(var sd in sides)
                {
                    var val = sd;
                    slope = val;
                    acc = calcacc(inputs,yindx,xindex);
                    if (acc > bestacc)
                    {
                        bestacc = acc;
                        indexbest = i;
                    }
                    i++;
                }
                slope = (sides[0] + sides[1]) / 2;
                var avg = slope;
                acc = calcacc(inputs, yindx, xindex);
                if (acc > bestacc)
                {
                    bestacc = acc;
                    indexbest = 2;
                    
                }
                i++;
                foreach (var sd in sides)
                {
                    var val = sd;
                    slope = (val+avg)/2;
                    acc = calcacc(inputs,yindx,xindex);
                    if (acc >= bestacc)
                    {
                        bestacc = acc;
                        indexbest = i;
                    }
                    i++;
                }
               
                double expand = (sides[0] - sides[1])/2;
                switch (indexbest)
                {
                    case 0:
                    case 1:
                        slope = sides[indexbest];
                        sides[0] = slope - expand*2;
                        sides[1] = slope + expand*2;
                        break;
                    case 2:
                        slope = avg;
                        sides[0] = slope - expand / 2;
                        sides[1] = slope + expand / 2;
                        break;
                    case 3:
                    case 4:
                        slope =( sides[indexbest-3]+avg)/2;
                        sides[0] = slope - expand ;
                        sides[1] = slope + expand;
                        break;
                }
                //
                //a
                //
                i = 0;
                foreach(var sd in sides2)
                {
                    var val = sd;
                    a = val;
                    acc = calcacc(inputs,yindx,xindex);
                    if (acc >= bestacc)
                    {
                        bestacc = acc;
                        indexbest = i;
                    }
                    i++;
                }
                a = (sides2[0] + sides2[1]) / 2;
                 avg = a;
                acc = calcacc(inputs, yindx, xindex);
                if (acc >= bestacc)
                {
                    bestacc = acc;
                    indexbest = 2;
                    
                }
                i++;
                foreach (var sd in sides2)
                {
                    var val = sd;
                    a = (val + avg) / 2;
                    acc = calcacc(inputs, yindx, xindex);
                    if (acc >= bestacc)
                    {
                        bestacc = acc;
                        indexbest = i;
                    }
                    i++;
                }
                 expand = (sides2[0] - sides2[1]) / 2;
                switch (indexbest)
                {
                    case 0:
                    case 1:
                        a = sides2[indexbest];
                        sides2[0] = a - expand * 2;
                        sides2[1] = a + expand * 2;
                        break;
                    case 2:
                        a = avg;
                        sides2[0] = a - expand / 2;
                        sides2[1] = a + expand / 2;
                        break;
                    case 3:
                    case 4:
                        a = (sides2[indexbest - 3] + avg)/2;
                        sides2[0] = a - expand;
                        sides2[1] = a + expand;
                        break;
                }
                var acc2 = calcacc(inputs, yindx, xindex);
                if (acc2 >= stopacc)
                {
                    break;
                }
            }
            
        }
        public override string ToString()
        {
            return "y=" + a + "+" + slope + "P";
        }
        public void fit2(List<double[]> inputs, int yindx = 0, int xindex = 1)
        {
            var slp = 0.0;
            var lastg = inputs[0];
            for (int i = 1; i < inputs.Count; i++)
            {
                var odt = (lastg[yindx] - inputs[i][yindx]) / (lastg[xindex] - inputs[i][xindex]);
                 lastg = inputs[i];
                slp += odt;
            }
            slope=slp / inputs.Count;
            double avga = 0.0;
            for (int i = 0; i < inputs.Count; i++)
            {
                var odt = inputs[i][yindx]- slope * inputs[i][xindex] ;
                avga += odt;
            }
            avga/= inputs.Count;
            a = avga;
            var f=  calcacc(inputs, yindx, xindex);
        }
        public static double[] getintersiction(line fl,line ll)
        {
            double x = (fl.a - ll.a) / (ll.slope - fl.slope);
            double y = fl.calc(x);
            y = ll.calc(x);
            double[] n = { x, y };
            return n;
        }
        public double calc(double x)
        {
            double y = a + slope * x;
            return y;
        }
        public double calcacc(List<double[]>inputs,int yindx=0,int xindex=1)
        {
            double acc = 0;
            foreach(var xy in inputs)
            {
                var realy = xy[yindx];
                var x = xy[xindex];
                var ty = calc(x);
                if (realy == 0)
                {
                    realy = 0.00001;
                }
                acc +=1- Math.Abs(ty - realy) / realy;
            }
            return acc * 100/inputs.Count;
        }
    }
}
