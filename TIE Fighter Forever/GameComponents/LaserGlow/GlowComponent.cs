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
using TIE_Fighter_Forever.Components;
using System.ComponentModel;


namespace TIE_Fighter_Forever.GameComponents.LaserGlow
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class GlowComponent : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Fields

        // Kommunik�ci�hoz a f�-j�t�k-oszt�llyal
        TIEGame game;

        // Teljesk�perny�s rajzol�shoz
        SpriteBatch spriteBatch;

        // Effect file a glowol� r�szek megk�l�nb�ztet�s�re
        Effect selectorEffect;
        // Effect file a gaussian blur v�grehajt�s�ra
        Effect gaussianBlurEffect;
        // Effect file a v�gleges k�p kialak�t�s�ra
        Effect finalEffect;

        // A jelenetet ide renderelj�k ki
        RenderTarget2D scene;
        // Ide j�n a megk�l�nb�ztetett r�szek renderje �s a m�sodik blur eredm�nye
        RenderTarget2D render0;
        // A megk�l�nb�ztetettet ide bluroljuk el�sz�r, majd ezt felhaszn�lva 
        // m�sodszor blurrolunk a m�sik ir�nyba
        RenderTarget2D render1;

        // A glow postprocess effect be�ll�t�sa
        GlowSetting setting = new GlowSetting(0.0f, 2.75f, 6.0f, 1.0f/*, 0.7f, 1.0f*/);

        public GlowSetting Setting
        {
            get { return setting; }
            set { setting = value; }
        }
        #endregion
        #region Initialization
        public GlowComponent(TIEGame game)
            : base(game)
        {
            if (game == null)
                throw new ArgumentNullException("game");
            else
                this.game = game;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Kell egy spritebatch a 2d rajzol�shoz
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Kellenek az effect fileok a shaderk�dokkal
            selectorEffect = Game.Content.Load<Effect>("Shaders\\PostProcess\\SelectGlowEffect");
            gaussianBlurEffect = Game.Content.Load<Effect>("Shaders\\PostProcess\\GaussianBlurEffect");
            finalEffect = Game.Content.Load<Effect>("Shaders\\PostProcess\\FinalGlowEffect");

            // Lek�rj�k a backbuffer tulajdons�gait
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;
            SurfaceFormat format = pp.BackBufferFormat;

            // Csin�lunk egy ugyanakkora puffert, mint a backbuffer(ebbe megy az eredeti renderk�p)
            scene = new RenderTarget2D(GraphicsDevice, width, height, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            width = (int)(width / game.settings.glowBadness);
            height = (int)(height / game.settings.glowBadness);

            // Majd csin�lunk k�t kissebb k�ztes buffert is a blurhoz
            render0 = new RenderTarget2D(GraphicsDevice, width, height, false, format, DepthFormat.None);
            render1 = new RenderTarget2D(GraphicsDevice, width, height, false, format, DepthFormat.None);

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            scene.Dispose();
            render0.Dispose();
            render1.Dispose();
            base.UnloadContent();
        }
        #endregion
        #region Draw
        /// <summary>
        /// Ezt a f�ggv�nyt kell megh�vni minden frameben m�g a kirajzol�sok el�tt!
        /// </summary>
        public void PrepareDraw()
        {
            // A visible tulajdons�g meghat�rozza lefut-e a draw h�v�s ha komponensk�nt
            // hozz� vagyunk adva, ezzel az if-el itt k�nnyen kezelhet�v� v�lik a glow
            // ki �s bekapcsol�sa, mert hiszen el�g a Visible-t �ll�tani!
            // Megj.: Lehet hogy az ifet kiszedem majd, de annyit nem lass�t egy ilyen
            // apr�s�g!
            if (Visible)
            {
                // Be�ll�tjuk, hogy a mi rendertarget�nkbe renderelj�nk...
                GraphicsDevice.SetRenderTarget(scene);
            }
        }
        /// <summary>
        /// Ez a f�ggv�ny h�vja meg a glow postprocessing t�nyleges meneteit �s 
        /// rajzolja ki a backbufferbe a t�nyleges k�sz k�pet is.
        /// </summary>
        /// <param name="gameTime">J�t�kbeli id�</param>
        public override void Draw(GameTime gameTime)
        {
            // ELS� MENET: Szelekci�.
            selectorEffect.Parameters["GlowBonus"].SetValue(setting.GlowBonus);
            // A kor�bban a scene-be renderelt k�p selekt�l�sa �s �tvitele render0-ba
            DrawFullscreenQuadToTarget(scene, render0, selectorEffect);

            // M�SODIK MENET: V�zszintes Gaussian Blur.
            SetBlurEffectParameters(1.0f / (float)render0.Width, 0);
            // A szelekt�lt k�pet render1-be renderelj�k �s v�zszintesen bluroljuk
            DrawFullscreenQuadToTarget(render0, render1, gaussianBlurEffect);

            // HARMADIK MENET: F�gg�leges Gaussian Blur.
            SetBlurEffectParameters(0, 1.0f / (float)render0.Width);
            // A v�zszintesen blurolt k�pet f�gg�legesen is bluroljuk, c�l: render0
            DrawFullscreenQuadToTarget(render1, render0, gaussianBlurEffect);

            // V�GS� MENET: Az eredeti scene �s a blurolt k�p �sszehoz�sa.
            // Be�ll�tjuk a backbuffert mint rendertargetet
            GraphicsDevice.SetRenderTarget(null);
            // Felparam�terezz�k a glow-t
            EffectParameterCollection parameters = finalEffect.Parameters;
            parameters["BaseIntensity"].SetValue(setting.BaseIntensity);
            parameters["GlowIntensity"].SetValue(setting.GlowIntensity);
            //parameters["BaseSaturation"].SetValue(setting.BaseSaturation);
            //parameters["GlowSaturation"].SetValue(setting.GlowSaturation);

            // Be�ll�tjuk a device-on a m�sodik text�r�t(az null�st a spritebatch fogja, egyest a normal map miatt nem cseszegetj�k!)
            // �s a hozz� tartoz� samplert (linear clamp az�rt kell, mert nem
            // kett�hatv�ny text�r�kn�l reach eset�n csak ez haszn�lhat�!)
            //SamplerState tmps0 = GraphicsDevice.SamplerStates[0];
            //SamplerState tmps1 = GraphicsDevice.SamplerStates[1];
            GraphicsDevice.SamplerStates[1] = game.settings.clampFilter;
            GraphicsDevice.Textures[1] = scene;

            // Kirenderelj�k a v�gleges, �sszetett k�pet
            Viewport viewport = GraphicsDevice.Viewport;
            DrawFullscreenQuad(render0, viewport.Width, viewport.Height, finalEffect);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Adott text�ra adott m�retben a k�perny�re renderel�se az adott effect-el!
        /// </summary>
        /// <param name="texture">Text�ra</param>
        /// <param name="width">Sz�less�g</param>
        /// <param name="height">Magass�g</param>
        /// <param name="effect">Haszn�lt effect</param>
        void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, effect);
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// Adott text�ra adott rendertargetbe renderel�se
        /// </summary>
        /// <param name="texture">Forr�stext�ra</param>
        /// <param name="target">C�lpont rendertarget</param>
        /// <param name="effect">Haszn�lt effect</param>
        void DrawFullscreenQuadToTarget(Texture2D texture, RenderTarget2D target,
                                        Effect effect)
        {
            GraphicsDevice.SetRenderTarget(target);
            DrawFullscreenQuad(texture, target.Width, target.Height, effect);
        }

        /// <summary>
        ///  A gaussg�rbe egy pontj�nak magass�g�t adja meg a bemeneti param�ter
        ///  f�ggv�ny�ben!
        /// </summary>
        /// <param name="n">abszcissza</param>
        /// <returns>oordin�ta</returns>
        float Gauss(float x)
        {
            // Megj.: A harangg�rbe cs�cs�nak magass�ga 1 / sqrt(2*PI*theta^2)
            // Megj.: A harangg�rbe "sz�less�ge" theta f�ggv�nye
            // A blurAmount teh�t a g�rb�t param�terezi a fenti k�t m�don is!
            float theta = setting.BlurAmount;
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta * theta)) *
                        Math.Exp(-(x * x) / (2 * theta * theta)));
        }

        void SetBlurEffectParameters(float dx, float dy)
        {
            // Az effect param�tereit az egyszer�s�g kedv��rt �ssze�ll�tjuk �s egyben
            // be�ll�tjuk!
            EffectParameter weightsParameter, offsetsParameter;
            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];
            
            // Illetve az is �rdekel minket, hogy h�ny elemet t�mogatunk a shader�nkben
            // samplel�si c�lokra...
            int sampleCount = weightsParameter.Elements.Count;

            // Ezekbe a t�mb�kbe ker�lnek a s�lyok �s az eltol�sok
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // Az els� �rt�kek mindig ezek(Gauss(0), nincs eltol�s)
            sampleWeights[0] = Gauss(0);
            sampleOffsets[0] = new Vector2(0);

            // A s�lyok �sszeg�nek t�rol�s�ra, hogy a v�g�n normaliz�lhassunk
            float weightSum = Gauss(0);

            // A sampleCount p�ratlan: egy k�z�ps� nulladik poz�ci� �s egy egyenes 
            // ment�n p�ros�val ide-oda l�p�get�s van. A k�z�ps�t kisz�moltunk fent
            // most a t�bbi j�n!
            for (int i = 0; i < sampleCount / 2; ++i)
            {
                // S�lyok kisz�m�t�sa
                float weight = Gauss(i + 1);
                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                // s�lyszumma n�vel�se
                weightSum += 2 * weight;

                // Eltol�sok kisz�m�t�sa:
                // kihaszn�ljuk, hogy a sampler hardware a k�t szomsz�dos sample 
                // �tlag�t adja meg akkor, ha pont a k�t sample k�z�l olvasunk a 
                // shaderb�l. Ez a b�nusz �tlagol�s sokat jav�t a blur-unk felbont�s�n!
                float offset = i * 2 + 1.5f;
                // Az elmozdul�s vektort �gy kre�ljuk, hogy a blur offseteinek
                // sz�mol�s�t a dx,dy param�terek alapj�n �ll�tsuk be(konkr�tan mi 
                // majd f�gg�leges �s v�zszintes elmozdul�sokat �ll�tunk be 1 �s 0-val)
                Vector2 delta = new Vector2(dx, dy) * offset;
                // Offsetek be�ll�t�sa a s�lyoknak megfelel�en
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normaliz�ljuk az eredm�nyt, azaz leosztjuk a s�lyokat, hogy a 
            // s�ly�sszeg 1 legyen!
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= weightSum;
            }

            // V�g�l mentj�k a kisz�molt param�tereket a hely�kre...
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }

        #endregion
    }
}
