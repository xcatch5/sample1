using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Sample1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Matrix worldMatrix;
        Matrix cameraMatrix;
        Matrix projectionMatrix;
        float angleHorisont = 200.14f;
        float angleVertical = -50f;
        float far = 0.2f;

        MouseState originalMouseState;

        BasicEffect cubeEffect;

        ModelImportShape model;
        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            model = new ModelImportShape(Content);
            
        }

        /// <summary>
        /// Allows the game to perform any initialization
        /// it needs to before starting to run.
        /// This is where it can query for any required 
        /// services and load any non-graphic
        /// related content.  Calling base. Initialize will
        /// enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            initializeWorld();
        }

        /// <summary>
        /// LoadContent will be called once 
        /// per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            model.shapeTexture = Content.Load<Texture2D>("Textures\\one_color");
            originalMouseState = Mouse.GetState();
        }

        /// <summary>
        /// UnloadContent will be called once per game
        /// and is the place to unload all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        
        /// <summary>
        /// Allows the game to run logic 
        /// such as updating the world,
        /// checking for collisions, 
        /// gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(
              PlayerIndex.One).Buttons.Back ==
              ButtonState.Pressed)
                this.Exit();

            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Right))
                angleHorisont += 0.9f;
            if (keyState.IsKeyDown(Keys.Left))
                angleHorisont -= 0.9f;
            if (keyState.IsKeyDown(Keys.Up))
                angleVertical += 0.5f;
            if (keyState.IsKeyDown(Keys.Down))
                angleVertical -= 0.5f;
            if (keyState.IsKeyDown(Keys.A))
                far *= 1.01f;
            if (keyState.IsKeyDown(Keys.Z))
                far *= 0.99f;
            /*
            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState != originalMouseState)
            {
                angleHorisont += (currentMouseState.X - originalMouseState.X)*0.1f;
                angleVertical += (currentMouseState.Y - originalMouseState.Y) * 0.1f;

                Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
                originalMouseState = Mouse.GetState();
            }
            */
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            cameraMatrix = Matrix.CreateLookAt(
             new Vector3(0, far * 50, far * 30),
             new Vector3(0, 0, 0),
             new Vector3(0, 1, 0));
            worldMatrix = Matrix.CreateRotationY(
              MathHelper.ToRadians(angleHorisont)) *
              Matrix.CreateRotationX(
                MathHelper.ToRadians(angleVertical));
            cubeEffect.World = worldMatrix;
            cubeEffect.View = cameraMatrix;

            #region light

            // primitive color
            cubeEffect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            cubeEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            cubeEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            cubeEffect.SpecularPower = 5.0f;
            cubeEffect.Alpha = 1.0f;

            cubeEffect.LightingEnabled = true;
            if (cubeEffect.LightingEnabled)
            {
                cubeEffect.DirectionalLight0.Enabled = true; // enable each light individually
                if (cubeEffect.DirectionalLight0.Enabled)
                {
                    // x direction
                    cubeEffect.DirectionalLight0.DiffuseColor = new Vector3(1, 0, 0); // range is 0 to 1
                    cubeEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, 0, 0));
                    // points from the light to the origin of the scene
                    cubeEffect.DirectionalLight0.SpecularColor = Vector3.One;
                }

                cubeEffect.DirectionalLight1.Enabled = true;
                if (cubeEffect.DirectionalLight1.Enabled)
                {
                    // y direction
                    cubeEffect.DirectionalLight1.DiffuseColor = new Vector3(0, 0.75f, 0);
                    cubeEffect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0, -1, 0));
                    cubeEffect.DirectionalLight1.SpecularColor = Vector3.One;
                }

                cubeEffect.DirectionalLight2.Enabled = true;
                if (cubeEffect.DirectionalLight2.Enabled)
                {
                    // z direction
                    cubeEffect.DirectionalLight2.DiffuseColor = new Vector3(0, 0, 0.5f);
                    cubeEffect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(0, 0, -1));
                    cubeEffect.DirectionalLight2.SpecularColor = Vector3.One;
                }
            }
            #endregion light


            foreach (EffectPass pass in
              cubeEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                cubeEffect.Texture = model.shapeTexture;
                model.RenderShape(GraphicsDevice);
                
            }



            base.Draw(gameTime);
        }


        public void initializeWorld()
        {
            
            cameraMatrix = Matrix.CreateLookAt(
              new Vector3(0, far * 50, far * 30),
              new Vector3(0, 0, 0),
              new Vector3(0, 1, 0));
            projectionMatrix =
              Matrix.CreatePerspectiveFieldOfView(
              MathHelper.PiOver4,
              Window.ClientBounds.Width /
              (float)Window.ClientBounds.Height, 1.0f, 500.0f);
            float tilt = MathHelper.ToRadians(22.5f);
            worldMatrix = Matrix.CreateRotationX(tilt) *
              Matrix.CreateRotationY(tilt);

            cubeEffect = new BasicEffect(GraphicsDevice);
            cubeEffect.World = worldMatrix;
            cubeEffect.View = cameraMatrix;
            cubeEffect.Projection = projectionMatrix;
            cubeEffect.TextureEnabled = true;
        }
    }
}
