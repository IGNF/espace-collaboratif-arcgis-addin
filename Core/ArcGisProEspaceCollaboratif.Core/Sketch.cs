using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ArcGisProEspaceCollaboratif.Core
{
    public class Sketch
    {
        ///	\brief Les différents types d'objet pour un croquis.
        public enum SketchType
        {
            Vide = 0,
            Point,      /*!< Pour un point du croquis. */
            Ligne,      /*!< Pour une polyligne du croquis. */ // Ne pas traduire car vient de l'API écrit en Français
            Polygone,   /*!< Pour un polygone simple (sans trous et non multiple) du croquis. */ // Ne pas traduire car vient de l'API écrit en Français
            Texte,      /*!< Pour un un champ texte du croquis. */
            Fleche,     /*!< Pour une flêche du croquis. */
            MultiPoint,
            MultiLigne,
            Multipolygone
        };

        /// <summary>
        /// Type du croquis
        /// </summary>
        public Sketch.SketchType Type = SketchType.Vide;

        /// <summary>
        /// Nom du croquis
        /// </summary>
        public string Name;

        /// <summary>
        /// La liste des attributs (clé,valeur)
        /// </summary>
        public List<SketchAttributes> Attributes = new List<SketchAttributes>();

        /// <summary>
        /// La liste des points composant le croquis (coordonnées)
        /// </summary>
        public List<Point> Points = new List<Point>();

        /// <summary>
        /// Constructeur d'un croquis
        /// </summary>
        /// <param nom="nom du croquis"></param>
        /// <param type="type du croquis"></param>
        /// 
        public Sketch()
        {
            this.Type = SketchType.Vide;
        }

        /// <summary>
        /// set le type
        /// </summary>
        /// <param name="type">type du croquis</param>
        public void SetType(SketchType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Ajoute un point à la liste des points du croquis
        /// </summary>
        /// <param point="le point à ajouter">un objet Point</param>        
        /// 
        public void AddPoint(Point point)
        {
            this.Points.Add(point);
        }

        /// <summary>
        /// Ajoute un attribut à la liste des attributs du croquis
        /// </summary>
        /// <param name="attribut">l'objet Attribut</param>
        public void AddAttribute(SketchAttributes attribute)
        {
            this.Attributes.Add(attribute);
        }

        /// <summary>
        /// Ajoute un attribut à la liste des attributs du croquis
        /// </summary>
        /// <param name="key">nom de l'attribut</param>
        /// <param name="value">valeur de l'attribut</param>
        public void AddAttribute(string key, string value)
        {
            SketchAttributes sketchAttributes = new SketchAttributes()
            {
                Name = key,
                Value = value
            };
            this.Attributes.Add(sketchAttributes);
        }

        /// <summary>
        /// Retourne le premier point de la liste
        /// </summary>
        /// <returns>le premier point</returns>
        public Point FirstCoord()
        {
            return this.Points.First(); ;
        }
 
        /// <summary>
        /// Retourne le dernier point de la liste
        /// </summary>
        /// <returns>le dernier point</returns>
        public Point LastCoord()
        {           
            return this.Points.Last();
        }

        /// <summary>
        /// Transforme le croquis en xml 
        /// </summary>
        /// <param name="doc">le document xml</param>
        /// <returns>le xml au format string</returns>
        public XDocument EncodeToXML(XDocument doc, XNamespace gml)
        { 
            XElement objet= new  XElement("objet", new XAttribute("type", this.Type.ToString()),
                                       new  XElement("nom", this.Name) );
            XElement geom =  new  XElement("geometrie");

            string coord="";
            foreach (Point pt in this.Points){
                coord += Convert.ToString(pt.Longitude, Constantes.invC) + "," + Convert.ToString(pt.Latitude, Constantes.invC) + " ";
            }
            coord = coord.Substring(0, coord.Length-1);

            switch ( this.Type) {
                case SketchType.Ligne: case SketchType.Fleche:
                    geom.Add(new XElement(gml +"LineString",
                                new XElement(gml + "coordinates", coord)
                            ));
                    break;

                case SketchType.Point: case SketchType.Texte:
                    geom.Add(new XElement(gml + "Point",
                                 new XElement(gml + "coordinates", coord)
                             ));
                    break;

                case SketchType.Polygone:
                     geom.Add(new XElement(gml +"Polygon",
                                new XElement(gml +"outerBoundaryIs",
                                   new XElement(gml +"LinearRing",
                                      new XElement(gml +"coordinates",coord)
                                   )
                                )
                             ));
                    break;
                default:
                    break;
            }
            objet.Add(geom);

            //Ajout des attributs
            XElement xattributs=new XElement("attributs"); ;
            foreach (SketchAttributes att in this.Attributes)
            {
                XElement xattribut = new XElement("attribut");
                xattribut.Add(new XAttribute("name", att.Name), att.Value);
                xattributs.Add(xattribut);   
            }

            objet.Add(xattributs);
            doc.Root.Add(objet);
            return doc;
        }
    }
}