using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content;

namespace MotherShipEffect1Pipeline
{
    /// <summary>
    /// Model processzor f�haj�khoz �s �r�llom�sokhoz, normal mapping t�mogat�ssal
    /// </summary>
    [ContentProcessor]
    public class MothershipEffect1ModelProcessor : ModelProcessor
    {
        /// <summary>
        /// A normal map auto be�ll�t�sa, vagy maga a norm�l map helye!
        /// </summary>
        private string normalMapTexture = "<auto>";
        [DisplayName("Normal Map Setting")]
        [DefaultValue("<auto>")]
        [Description("The normal map applied to the model. All <texture>_bump.* files became normal map for a material if you set this to <auto>!")]
        public string NormalMapTexture
        {
            get { return normalMapTexture; }
            set { normalMapTexture = value; }
        }

        /// <summary>
        /// Fel�ldefini�ljuk ezt a property-t, mert nek�nk MINDIG sz�ks�g�nk
        /// van a gener�lt tangensekre!
        /// </summary>
        [Browsable(false)]
        public override bool GenerateTangentFrames
        {
            get { return true; }
            set { }
        }

        public override ModelContent Process(NodeContent input,
            ContentProcessorContext context)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            //directory = Path.GetDirectoryName(input.Identity.SourceFilename);

            if (!(NormalMapTexture.Equals("<auto>")) && (NormalMapTexture == null))
            {
                throw new InvalidContentException("Error finding normal map!");
            }

            return base.Process(input, context);
        }

        /// <summary>
        /// Ezek a sz�munkra is fontos adatok a vertex csatorn�b�l.
        /// A t�bbit eldobjuk lejjebb!
        /// </summary>
        static IList<string> acceptableVertexChannelNames =
            new string[]
            {
                VertexChannelNames.TextureCoordinate(0),
                VertexChannelNames.Normal(0),
                VertexChannelNames.Binormal(0),
                VertexChannelNames.Tangent(0)
            };

        /// <summary>
        /// Ezt fel�ldefini�ltuk, hogy csak azok a vertex csatorn�k 
        /// maradjanak ami felt�tlen�l sz�ks�ges
        /// </summary>
        /// <param name="geometry">A geometria objektum ami tartalmaza
        /// a csatorn�t</param>
        /// <param name="vertexChannelIndex">A vertex csatorna indexe</param>
        /// <param name="context">A k�rnyezet amiben dolgozunk
        /// (pl. lehetne jelezni, hogy elt�vol�t�sra ker�lnek dolgok)</param>
        protected override void ProcessVertexChannel(GeometryContent geometry,
                    int vertexChannelIndex, ContentProcessorContext context)
        {
            String vertexChannelName = geometry.Vertices.Channels[vertexChannelIndex].Name;

            // Ha a fenti felsorol�sban megtal�lhat� a csatornan�v, akkor mehet
            if (acceptableVertexChannelNames.Contains(vertexChannelName))
            {
                base.ProcessVertexChannel(geometry, vertexChannelIndex, context);
            }
            // egy�bk�nt t�vol�tsuk el, csup�n felesleges adat van ott ilyenkor!
            else
            {
                geometry.Vertices.Channels.Remove(vertexChannelName);
            }
        }

        /// <summary>
        /// Az anyagok �talak�t�s�hoz a MothershipEffect1MaterialProcessort
        /// kell haszn�lni, be�ll�tjuk, hogy ezt haszn�ljuk.
        /// </summary>
        protected override MaterialContent ConvertMaterial(MaterialContent material,
                                                         ContentProcessorContext context)
        {
            // Egy dictionary a processzor param�terez�s�re, itt adjuk hozz�
            // a normalmapot is...
            OpaqueDataDictionary processorParameters = new OpaqueDataDictionary();
            // Megj.: Ezeknek public propertynek kell lenni a processzorban!
            // (convert�l�skor �r�dnak be innen!)
            processorParameters["NormalMapTexture"] = NormalMapTexture;
            processorParameters["ColorKeyColor"] = ColorKeyColor;
            processorParameters["ColorKeyEnabled"] = ColorKeyEnabled;
            processorParameters["TextureFormat"] = TextureFormat;
            processorParameters["GenerateMipmaps"] = GenerateMipmaps;
            processorParameters["ResizeTexturesToPowerOfTwo"] = ResizeTexturesToPowerOfTwo;

            // Haszn�ljuk a saj�t processzort a fenti param�terekkel
            return context.Convert<MaterialContent, MaterialContent>
                (material, "MothershipEffect1MaterialProcessor", processorParameters);
        }
    }
}