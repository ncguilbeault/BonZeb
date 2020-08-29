#version 400

uniform float phase;
uniform int switch_stim = 0;

const float angle = 0.0;
const float speed = 1.5;
const float frequency = 4.3;
const float twopi = 6.28318530718;

in vec2 tex_coord;
out vec4 frag_colour;

void black() {

  fragColour = vec4(0, 0, 0, 0);
  return;

}

void sinusoidal_gratings() {

  float value = 0.5f * sin(((texCoord.x - 0.5) * cos(angle) + ((texCoord.y - 0.5) * sin(angle))) * twopi * frequency + speed * phase) + 0.5f;
  fragColour = vec4(value, value, value, 1);
  return;

}

void square_wave_gratings() {

  float value = 0.5f * sin(((texCoord.x - 0.5) * cos(angle) + ((texCoord.y - 0.5) * sin(angle))) * twopi * frequency + speed * phase) + 0.5f;
  if (value >= 0.5)
  {
    fragColour = vec4(1, 1, 1, 1);
  }
  else
  {
    fragColour = vec4(0, 0, 0, 0);
  }
  return;

}

void main() {

  switch (switch_stim) 
  {

    case 0:
      black();
      return;

    case 1:
      square_wave_gratings();
      return;

    case 2:
      sinusoidal_gratings();
      return;

  }

}
