using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace TIE_Fighter_Forever.GameComponents.SkyBox
{
    /// <summary>
    /// SkyBox komponens, param�terezhet� text�r�kkal
    /// </summary>
    public class SkyBox : Microsoft.Xna.Framework.DrawableGameComponent
    {
        // Ez kell a f� j�t�kkal val� kommunik�ci� miatt
        TIEGame game;
        String skyBoxPath;
        // 6 text�ra a skybox oldalaihoz
        Texture2D[] textures = new Texture2D[6];
        // Effect file a skybox megjelen�t�s�hez
        Effect effect;
        // Vertex buffer, mely a skybox vertexeit fogja tartalmazni
        VertexBuffer vertices;
        // Index buffer a skybox vertexein, mert egy oldal k�t h�romsz�g �s 
        // �gy sp�rolhatunk a vertexek sz�m�n egy kicsit
        IndexBuffer indices;
        // Skybox kiterjeszt�se
        Vector3 vExtents;
        // Kamera ir�nyvektora �s helye
        Vector3 vCameraDirection;
        // M�trixok
        Matrix mView, mProjection;

        // Publikus property-k
        public Vector3 CameraDirection
        {
            get { return vCameraDirection; }
            set { vCameraDirection = value; }
        }

        public Matrix view
        {
            set { mView = value; }
        }

        public Matrix projection
        {
            set { mProjection = value; }
        }

        /// <summary>
        /// Csin�l egy skybox komponenst, az adott text�r�kkal �s kiterjed�ssel
        /// </summary>
        /// <param name="game">A f� j�t�k objektum</param>
        /// <param name="skyBoxName">A SkyBoxes\\%skyboxname\\back.jpg... helyen keress�k a text�r�kat</skyboxname></param>
        /// <param name="vExtents">A Skybox kiterjed�se a 3 dimenzi�ban</param>
        public SkyBox(TIEGame game, string skyBoxName, Vector3 vExtents)
            : base(game)
        {
            // �ssze�ll�tjuk a Skybox text�r�k path-j�t a param�ter alapj�n
            this.skyBoxPath = "SkyBoxes\\" + skyBoxName + "\\";
            // M�retvektor ment�se
            this.vExtents = vExtents;
            // Mentj�k a TIEGame oszt�lyunk, kommunik�ci� eset�re
            this.game = game;
        }

        /// <summary>
        /// Bet�lti a skybox contentjeit �s inicializ�lja a skyboxot
        /// </summary>
        public override void Initialize()
        {
            ContentManager content = game.Content;
            // Text�r�k bet�lt�se param�terrel meghat�rozott Path-r�l
            textures[0] = content.Load<Texture2D>(skyBoxPath + "front");
            textures[1] = content.Load<Texture2D>(skyBoxPath + "top");
            textures[2] = content.Load<Texture2D>(skyBoxPath + "right");
            textures[3] = content.Load<Texture2D>(skyBoxPath + "left");
            textures[4] = content.Load<Texture2D>(skyBoxPath + "bottom");
            textures[5] = content.Load<Texture2D>(skyBoxPath + "back");

            // Haszn�lt effect bet�lt�se
            effect = content.Load<Effect>("Shaders\\SkyBox");

            // Lek�rj�k a j�t�k oszt�lyunkt�l a graphicsdevice-t, mert
            // abb�l kinyerhet� a graphicsdevice majd.
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)
                game.Services.GetService(typeof(IGraphicsDeviceService));

            // Vertexdeklar�ci� amit haszn�lni fogunk
            VertexDeclaration vDecl = new VertexDeclaration(new VertexElement[]{
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate,0)});
            // L�trehozzuk a vertex buffert
            vertices = new VertexBuffer(graphicsService.GraphicsDevice, vDecl, 4 * 6, BufferUsage.WriteOnly);

            // Ide sz�moljuk ki a skybox vertexeinek koordin�t�it �s text�rakordin�t�it
            VertexPositionTexture[] data = new VertexPositionTexture[4 * 6];

            // VERTEX BUFFER FELT�LT�SE VERTEXKOORDIN�T�KKAL �S TEXT�RAKOORDIN�T�KKAL
            // front face
            data[0].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
            data[0].TextureCoordinate.X = 0.0f; data[0].TextureCoordinate.Y = 0.0f;
            data[1].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
            data[1].TextureCoordinate.X = 1.0f; data[1].TextureCoordinate.Y = 0.0f;
            data[2].Position = new Vector3(vExtents.X, -vExtents.Y, -vExtents.Z);
            data[2].TextureCoordinate.X = 1.0f; data[2].TextureCoordinate.Y = 1.0f;
            data[3].Position = new Vector3(-vExtents.X, -vExtents.Y, -vExtents.Z);
            data[3].TextureCoordinate.X = 0.0f; data[3].TextureCoordinate.Y = 1.0f;
            // top face
            data[4].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
            data[4].TextureCoordinate.X = 0.0f; data[4].TextureCoordinate.Y = 0.0f;
            data[5].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
            data[5].TextureCoordinate.X = 1.0f; data[5].TextureCoordinate.Y = 0.0f;
            data[6].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
            data[6].TextureCoordinate.X = 1.0f; data[6].TextureCoordinate.Y = 1.0f;
            data[7].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
            data[7].TextureCoordinate.X = 0.0f; data[7].TextureCoordinate.Y = 1.0f;
            // right face
            data[8].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
            data[8].TextureCoordinate.X = 0.0f; data[8].TextureCoordinate.Y = 0.0f;
            data[9].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
            data[9].TextureCoordinate.X = 1.0f; data[9].TextureCoordinate.Y = 0.0f;
            data[10].Position = new Vector3(vExtents.X, -vExtents.Y, vExtents.Z);
            data[10].TextureCoordinate.X = 1.0f; data[10].TextureCoordinate.Y = 1.0f;
            data[11].Position = new Vector3(vExtents.X, -vExtents.Y, -vExtents.Z);
            data[11].TextureCoordinate.X = 0.0f; data[11].TextureCoordinate.Y = 1.0f;
            // left face
            data[12].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
            data[12].TextureCoordinate.X = 0.0f; data[12].TextureCoordinate.Y = 0.0f;
            data[13].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
            data[13].TextureCoordinate.X = 1.0f; data[13].TextureCoordinate.Y = 0.0f;
            data[14].Position = new Vector3(-vExtents.X, -vExtents.Y, -vExtents.Z);
            data[14].TextureCoordinate.X = 1.0f; data[14].TextureCoordinate.Y = 1.0f;
            data[15].Position = new Vector3(-vExtents.X, -vExtents.Y, vExtents.Z);
            data[15].TextureCoordinate.X = 0.0f; data[15].TextureCoordinate.Y = 1.0f;            
            // bottom face
            data[16].Position = new Vector3(-vExtents.X, -vExtents.Y, -vExtents.Z);
            data[16].TextureCoordinate.X = 0.0f; data[16].TextureCoordinate.Y = 0.0f;
            data[17].Position = new Vector3(vExtents.X, -vExtents.Y, -vExtents.Z);
            data[17].TextureCoordinate.X = 1.0f; data[17].TextureCoordinate.Y = 0.0f;
            data[18].Position = new Vector3(vExtents.X, -vExtents.Y, vExtents.Z);
            data[18].TextureCoordinate.X = 1.0f; data[18].TextureCoordinate.Y = 1.0f;
            data[19].Position = new Vector3(-vExtents.X, -vExtents.Y, vExtents.Z);
            data[19].TextureCoordinate.X = 0.0f; data[19].TextureCoordinate.Y = 1.0f;                        
            // back face
            data[20].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
            data[20].TextureCoordinate.X = 0.0f; data[20].TextureCoordinate.Y = 0.0f;
            data[21].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
            data[21].TextureCoordinate.X = 1.0f; data[21].TextureCoordinate.Y = 0.0f;
            data[22].Position = new Vector3(-vExtents.X, -vExtents.Y, vExtents.Z);
            data[22].TextureCoordinate.X = 1.0f; data[22].TextureCoordinate.Y = 1.0f;
            data[23].Position = new Vector3(vExtents.X, -vExtents.Y, vExtents.Z);
            data[23].TextureCoordinate.X = 0.0f; data[23].TextureCoordinate.Y = 1.0f;            
            
            // Az adatunkat betessz�k a vertexbuffer objektumba
            vertices.SetData<VertexPositionTexture>(data);

            // INDEX BUFFER FELT�LT�SE:
            // Egy oldal k�t h�romsz�gb�l �ll, ami 6*6=36 darab indexet eredm�nyez(a vertexpuffer viszont csak 24 elem�, itt a sp�rol�s)
            indices = new IndexBuffer(graphicsService.GraphicsDevice, typeof(short), 6 * 6, BufferUsage.WriteOnly);
            // csin�lunk egy t�mb�t az indexek el��ll�t�s�hoz
            short[] ib = new short[6 * 6];
            // el��ll�tjuk az indexeket minden kockaoldalra
            for (int i = 0; i < 6; ++i)
            {
                // A Vertex bufferben egy oldalhoz 4 adat(4-el szorz�s)
                // Az index bufferben egy oldalhoz 6 adat(6-al szorz�s)
                // Els� h�romsz�g
                ib[i * 6 + 0] = (short)(i * 4 + 0);
                ib[i * 6 + 1] = (short)(i * 4 + 1);
                ib[i * 6 + 2] = (short)(i * 4 + 2);
                // M�sodik h�romsz�g
                ib[i * 6 + 3] = (short)(i * 4 + 0);
                ib[i * 6 + 4] = (short)(i * 4 + 2);
                ib[i * 6 + 5] = (short)(i * 4 + 3);
            }
            // Az eredm�nyt az index buffer objektumba tessz�k
            indices.SetData<short>(ib);

            base.Initialize();
        }

        protected override void UnloadContent()
        {
            vertices.Dispose();
            base.UnloadContent();
        }

        /// <summary>
        ///  A Skybox kirajzol�sa
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Ha v�letlen a komponenst hozz�adn�k Initialize n�lk�l
            if (vertices == null)
                return;

            // Nem a shaderben m�trixszorzunk vertexenk�nt, hanem itt egyben �tadjuk
            Matrix temp = mView * mProjection;
            temp.M41 = 0.0f;
            temp.M42 = 0.0f;
            temp.M43 = 0.0f;
            temp.M44 = 1.0f;
            effect.Parameters["rot"].SetValue(temp);

            for (int x = 0; x < 6; x++)
            {
                float f = 0;
                switch (x)
                {
                    // Leellen�r�zz�k, hogy egy�ltal�n fog-e l�tszani az adott face:
                    // XNA nem szereti a sok drawuserprimitives h�v�st, ez�rt 
                    // fontos ezt CPU-n ellen�r�zni!
                    case 0: //front
                        f = Vector3.Dot(vCameraDirection, new Vector3(0, 0, -1));
                        break;
                    case 1: //top
                        f = Vector3.Dot(vCameraDirection, new Vector3(0, -1, 0));
                        break;
                    case 2: //right
                        f = Vector3.Dot(vCameraDirection, new Vector3(-1, 0, 0));
                        break;
                    case 3: //left
                        f = Vector3.Dot(vCameraDirection, new Vector3(1, 0, 0));
                        break;
                    case 4: //bottom
                        f = Vector3.Dot(vCameraDirection, new Vector3(0, 1, 0));
                        break;
                    case 5: //back
                        f = Vector3.Dot(vCameraDirection, new Vector3(0, 0, 1));
                        break;
                }

                // Ha l�tszik az adott face:
                //if (f <= 0)
                if(true)
                {
                    // Majd kell a graphicsdevice
                    IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)
                        Game.Services.GetService(typeof(IGraphicsDeviceService));
                    GraphicsDevice device = graphicsService.GraphicsDevice;

                    // Be�ll�tjuk a haszn�lt vertexbuffert
                    device.SetVertexBuffer(vertices);
                    // �s indexbuffert
                    device.Indices = indices;

                    // be�ll�tjuk az adott face text�r�j�t
                    effect.Parameters["baseTexture"].SetValue(textures[x]);

                    // aktiv�ljuk az effectet
                    effect.Techniques[0].Passes[0].Apply();

                    // v�g�l kirajzoljuk a face-t
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                 0, x * 4, 4, x * 6, 2);
                }
            }
            
            base.Draw(gameTime);
        }
    }
}
