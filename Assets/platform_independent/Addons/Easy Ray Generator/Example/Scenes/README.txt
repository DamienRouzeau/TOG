----------------------------------
Easy Ray Generator (v1.0)
Author: Daniele Olivieri
Date: 02/11/2016
----------------------------------


----------
DEMO SCENE
----------
Open the Example.unity and press Play in the editor,
use the mouse to look around and the following keys to move in the environment:

W: move forward
A: move Left
D: move right
S: move backward
Space: Activate / Deactivate the laser

The environment scene is from "Mecanim Example Scenes" Unity package.


----------------------------
HOW TO USE THE RAY GENERATOR
----------------------------

Drop the Laser prefab in your project and move it under your "gun object",
the beam will start from the "Beam StartPoint" toward its local z-axis direction.
Reference the LaserControl in your script and enable or disable the ray.
Example code:

public LaserControl laserControl;

laserControl.Activate = true;
laserControl.Activate = false;

You have also another option to deactivate totally the laser, in case of pausing the game:

laserControl.EnableLaser = true;
laserControl.EnableLaser = false;


LASER LENGTH
------------

In the Laser Prefab you can update the parameter Max Length, this is the number of units for the max length of the ray.

LASER TEXTURE
-------------

If you prefer to change the colour or the shape of the ray, you need to update the texture in the "Laser Shot" material. To avoid gaps in the ray it must be horizontal tileable.
To change the texture of the burning point, you can update the textures in the following materials: ParticleFirework, ParticleSmokeMobile, ParticleSpark.
And also updating the values in the Particle System components under the "Flare" object.







