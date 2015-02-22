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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using TIE_Fighter_Forever.Screens.Menu;
using TIE_Fighter_Forever.Input;
using TIE_Fighter_Forever.Screens.Battle;

namespace TIE_Fighter_Forever
{
    /// <summary>
    /// Ez a j�t�k f� oszt�lya, mely a bel�p�si pontn�l futtatva van
    /// </summary>
    public class TIEGame : Microsoft.Xna.Framework.Game
    {
        public Settings settings;
        public GraphicsDeviceManager graphics;

        public TIEGame()
        {
            // Alapbe�ll�t�sok bet�lt�se
            settings = new Settings();

            // �s �rv�nyes�t�se
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = settings.preferredScreenWidth;
            graphics.PreferredBackBufferHeight = settings.preferredScreenHeight;
            graphics.PreferredDepthStencilFormat = settings.preferredDepthStencil;
            graphics.IsFullScreen = settings.isFullScreen;
            graphics.PreferMultiSampling = settings.multiSampling;
            // ez az�rt kell, mert egy�bk�nt ritk�bb a draw h�v�s az update-ek jav�ra
            // Ha false-ra teszem, akkor painkiller-es jelleg� hat�st v�lt ki, ami jobb mint az akad�s!
            this.IsFixedTimeStep = true;

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Itt inicializ�ljuk a j�t�kot.
        /// A base.Initialize() megh�vja az �sszes gyermek komponens inicializ�ci�j�t is, 
        /// persze csak azok�t, amik m�r hozz� vannak adva komponensk�nt!
        /// </summary>
        protected override void Initialize()
        {
            // Backface Culling:
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone; // .CullCounterClockwise;

            KeyboardManager keyMan = new KeyboardManager(this);
            MouseManager mouseMan = new MouseManager(this);
            mouseMan.setScreenWidth(graphics.PreferredBackBufferWidth);
            mouseMan.setScreenHeight(graphics.PreferredBackBufferHeight);
            MainMenuScreen mainMenu = new MainMenuScreen(this);
            // Tesztel�s:
            //BattleScreen mainMenu = new BattleScreen(this, null);

            /*keyMan.UpdateOrder = 0;
            mouseMan.UpdateOrder = 1;
            mainMenu.UpdateOrder = 10;*/

            this.Components.Add(keyMan);
            this.Components.Add(mouseMan);
            this.Components.Add(mainMenu);

            base.Initialize();
        }

        /// <summary>
        /// Itt t�ltj�k be a grafikus contentet
        /// (az egyes k�perny�k maguknak t�ltik ezt itt ez�rt �res)
        /// </summary>
        protected override void LoadContent()
        {
            // Contents are loaded in the screen classes
        }

        /// <summary>
        /// Ez j�t�konk�nt egyszer h�v�dik meg �s itt kell unloadolni mindent,
        /// amit nem content managerrel loadolunk...
        /// </summary>
        protected override void UnloadContent()
        {
            // Jelenleg mindent a content pipeline t�lt be, itt nem �ll semmi...
        }

        /// <summary>
        /// Itt t�rt�nik az update, a j�t�klogika.
        /// Az �sszes update a komponensekben t�rt�nik, 
        /// amit a base.Update() h�v meg minden akt�v komponensre!
        /// </summary>
        /// <param name="gameTime">Id�m�r�sre haszn�lhat� �rt�k</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Itt t�rt�nik a kirajzol�s.
        /// Minden kirajzol�s a komponensekben zajlik, innen a base.Draw() h�vja �ket.
        /// </summary>
        /// <param name="gameTime">Id�m�r�sre haszn�lhat� �rt�k</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
