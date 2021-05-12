using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Représente un Point EspaceCollaboratif donné en longitude/latitude
    /// </summary>
    public class Point
    {
        /// <summary>
        /// La longitude (WGS84 en degrés décimaux) du point
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// La latitude (WGS84 en degrés décimaux) du point
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Constructeur d'un point vide
        /// </summary>
        public Point()
        {
            this.Longitude = double.NaN ;
            this.Latitude  = double.NaN ;
        }

        /// <summary>
        /// Constructeur à partir d'une longitude/latitude
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        public Point(double lon, double lat)
        {
            this.Longitude = lon;
            this.Latitude = lat;
        }

        /// <summary>
        /// Test si le point est vide
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty(){
            return double.IsNaN(this.Longitude) || double.IsNaN(this.Latitude);
        }

        public override bool Equals(Object obj)
        {
            return (this.Longitude == ((Point)obj).Longitude) && (this.Latitude == ((Point)obj).Latitude);
        }

        public override int GetHashCode()
        {
            return (int)(this.Longitude + this.Latitude);
        } 
     
        /// <summary>
        /// Get point as WKT
        /// </summary>
        /// <returns></returns>
        public string AsText(){
            if (this.IsEmpty())
            {
                return "POINT EMPTY";
            }
            else
            {
                return "POINT(" + this.Longitude + " " + this.Latitude + ")";
            }
        }
    }
}
