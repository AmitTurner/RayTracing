# Ray Tracing
A fragment shader with lights and shadows test.
After recieving a 3d scene, the shader creates the accurate picture with shadows and lights.

🌄 Background:

  o Plain color background

💠 Display geometric primitives in space:

  o Spheres

  o Planes

💡 Basic lighting:

  o Directional lights

  o Spot lights

  o Ambient light

  o Simple materials (ambient, diffuse, specular...)

👥 Basic hard shadows

🪞 One Reflection (default as turned off)

Input is considered to be arrays with the objects already parsed.
Light direction will describe by light direction (x, y, z, w). 'w' value
will be 0.0 for directional light and 1.0 for spotlight.
- For spotlights the position will in lightPosition[] (x,y,z,w).
- Light intensity will describe by light intensity (R, G, B, A).
- Spheres and planes in objects[]: For spheres (x,y,z,r) when
(x,y,z) is the center position and r is the radius (always positive, greater than zero). For
planes (a,b,c,d) which represents the coefficients of the plane equation when 'd' gets is
a non-positive value.
- The color of an object will appears in objColors[] will represents the ambient and diffuse
values (R,G,B,A). 'A' represents the shininess parameter value

Examples created by the shader:
![alt text](https://raw.githubusercontent.com/AmitTurner/RayTracing/master/scene0.png)
![alt text](https://raw.githubusercontent.com/AmitTurner/RayTracing/master/scene1.png)
![alt text](https://raw.githubusercontent.com/AmitTurner/RayTracing/master/scene2.png)
