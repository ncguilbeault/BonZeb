#version 400

// Define calibration parameters received from Bonsai workflow
uniform float xOffset;
uniform float yOffset;
uniform float xRange;
uniform float yRange;

// Define OpenGL incoming and outgoing variables
in vec2 vp;
in vec2 vt;
out vec2 tex_coord;

void main()
{
  gl_Position = vec4((vp.x * xRange) + xOffset, (vp.y * yRange) + yOffset, 0.0, 1.0);
  tex_coord = vt;
}
