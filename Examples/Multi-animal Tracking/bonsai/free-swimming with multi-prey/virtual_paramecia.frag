#version 400
uniform int switch_stim;
in float orientation;
out vec4 frag_color;

void main()
{

  switch (switch_stim)
  {
    case 0:
      frag_color = vec4(0.0, 0.0, 0.0, 0.0);
      return;
    case 1:
      // position of the fragment inside the unit circle
      vec2 position = 2 * gl_PointCoord - 1;

      // fragment is inside the circle when the length is smaller than one
      float scale = sqrt(pow(position[0] - (cos(orientation) * 0.9), 2) + pow(position[1] - (sin(orientation) * 0.9), 2)) + sqrt(pow(position[0] + (cos(orientation) * 0.9), 2) + pow(position[1] + (sin(orientation) * 0.9), 2)) < 2.0 ? 1.0 : 0.0;
      frag_color = vec4(1.0, 1.0, 1.0, 1.0) * scale;
      return;
  }
}
