using System;

namespace EspaceCollaboratif.Core
{
    /// <summary>
    /// Représente une boite englobante
    /// </summary>
    public class Box
    {

        public double XMin ;
        public double YMin ;
        public double XMax ;
        public double YMax ;



        /// <summary>
        /// Constructeur d'une boite vide
        /// </summary>
        public Box()
        {
            this.XMin = double.NaN;
            this.YMin = double.NaN;
            this.XMax = double.NaN;
            this.YMax = double.NaN; 
        }

        /// <summary>
        /// Constructeur à partir de deux points 
        /// </summary>
        /// <param name="xMin">coord X min</param>
        /// <param name="yMin">coord Y min</param>
        /// <param name="xMax">coord X max</param>
        /// <param name="yMax">coord Y max</param>
        public Box(double xMin, double yMin, double xMax, double yMax)
        {
            if (xMin < xMax)
            {
                this.XMin = xMin;
                this.XMax = xMax;
            }
            else
            {
                this.XMin = xMax;
                this.XMax = xMin;
            }
            if (yMin < yMax)
            {
                this.YMin = yMin;
                this.YMax = yMax;
            }
            else
            {
                this.YMin = yMax;
                this.YMax = yMin;
            }
        }

        /// <summary>
        /// Constructeur de copie
        /// </summary>
        public Box(Box other)
        {
            this.XMin = other.XMin;
            this.YMin = other.YMin;
            this.XMax = other.XMax;
            this.YMax = other.YMax;
        }


        /// <summary>
        /// Test si la boite est vide 
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return double.IsNaN(this.XMin) || double.IsNaN(this.YMin) || double.IsNaN(this.XMax) || double.IsNaN(this.YMax);
        }


        public String BoxToString(){
            String strBox = Convert.ToString(this.XMin, ConstanteEspaceCollaboratif.invC)+ "," +
                            Convert.ToString(this.YMin, ConstanteEspaceCollaboratif.invC) + "," +
                            Convert.ToString(this.XMax, ConstanteEspaceCollaboratif.invC) + "," +
                            Convert.ToString(this.YMax, ConstanteEspaceCollaboratif.invC) ;
            return strBox;
        }

    }
}