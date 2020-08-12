#version 400

// Black and White Flashes parameters
const float black_white_flashes_dur = 0.2; // black and white flashes duration

// Looming Dot Parameters
const float looming_dot_dist = 0.1; // distance of looming dot from fish
const float looming_dot_lv = 0.03; // size to velocity ratio. Larger values take longer to expand than smaller values
const float looming_dot_exp = 3.0; // looming dot time to full expansion
const float looming_dot_stop = 2.85; // looming dot time until expansion stops. Value must be less than looming_dot_exp. Smaller values cause looming dot to stop prematurely

// Optomotor Gratings Parameters
const float optomotor_gratings_speed = 4.0; // speed of optomotor gratings
const float optomotor_gratings_spat_freq = 4.0; // spatial frequency of optomotor gratings

// Moving Prey Parameters
const float moving_prey_dist = 0.1; // distance of moving prey from fish
const float moving_prey_speed = 0.6; // speed of moving prey
const float moving_prey_size = 0.002; // size of moving prey

// Stationary Prey Parameters
const float stationary_prey_dist = 0.1; // distance of stationary prey from fish
const float stationary_prey_size = 0.002; // size of stationary prey

// Optokinetic Gratings Parameters
const float optokinetic_gratings_speed = 4.0; // speed of optokinetic gratings
const float optokinetic_gratings_spat_freq = 4.0; // spatial frequency of optokinetic gratings
const float optokinetic_gratings_inner_rad = 0.01; // radius of the black inner circle
const float optokinetic_gratings_outer_rad = 0.5; // radius of the optokinetic gratings

// Define useful variable constants
const float quarterpi = 0.78539816339;
const float thirdpi = 1.0471975512;
const float halfpi = 1.57079632679;
const float pi = 3.14159265359;
const float threehalvespi = 4.71238898038;
const float twopi = 6.28318530718;
const float inf = 1. / 0.;

// Define uniform variables incoming from Bonsai
uniform float fish_position_x;
uniform float fish_position_y;
uniform float fish_heading_angle;
uniform float time;
uniform int stimulus_number;

// Define OpenGL incoming and outgoing variables
in vec2 tex_coord;
out vec4 frag_colour;

// Define solid black
void solid_black()
{
  frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  return;
}

// Define solid white
void solid_white()
{
  frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  return;
}

// Define black and white flashes
void black_white_flashes()
{
  if (int(time / black_white_flashes_dur) % 2 == 0)
  {
    frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  }
  else
  {
    frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
  return;
}

// Define left phototaxis
void left_phototaxis()
{
  if ((tex_coord.x - fish_position_x) * sin(fish_heading_angle + halfpi) + (tex_coord.y - fish_position_y) * cos(fish_heading_angle + halfpi) < 0)
  {
    frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  }
  else
  {
    frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
}

// Define right phototaxis
void right_phototaxis()
{
  if ((tex_coord.x - fish_position_x) * sin(fish_heading_angle + threehalvespi) + (tex_coord.y - fish_position_y) * cos(fish_heading_angle + threehalvespi) < 0)
  {
    frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  }
  else
  {
    frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
}

// Define left looming dot
void left_looming_dot()
{
  vec2 circle_position = vec2(fish_position_x + looming_dot_dist * sin(fish_heading_angle - thirdpi), fish_position_y + looming_dot_dist * cos(fish_heading_angle - thirdpi));
  float size = time < looming_dot_stop ? -looming_dot_lv / (time - looming_dot_exp) : looming_dot_stop < looming_dot_exp ? looming_dot_lv / (looming_dot_exp - looming_dot_stop) : inf;
  if (sqrt(pow(tex_coord.x - circle_position[0], 2) + pow(tex_coord.y - circle_position[1], 2)) <= size)
  {
    frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
  else
  {
    frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  }
  return;
}

// Define right looming dot
void right_looming_dot()
{
  vec2 circle_position = vec2(fish_position_x + looming_dot_dist * sin(fish_heading_angle + thirdpi), fish_position_y + looming_dot_dist * cos(fish_heading_angle + thirdpi));
  float size = time < looming_dot_stop ? -looming_dot_lv / (time - looming_dot_exp) : looming_dot_stop < looming_dot_exp ? looming_dot_lv / (looming_dot_exp - looming_dot_stop) : inf;
  if (sqrt(pow(tex_coord.x - circle_position[0], 2) + pow(tex_coord.y - circle_position[1], 2)) <= size)
  {
    frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
  else
  {
    frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  }
  return;
}

// Define left optomotor gratings
void left_optomotor_gratings()
{
  float value = 0.5 * sin(((tex_coord.x - fish_position_x) * sin(fish_heading_angle + threehalvespi) + (tex_coord.y - fish_position_y) * cos(fish_heading_angle + threehalvespi)) * twopi * optomotor_gratings_spat_freq - optomotor_gratings_speed * time) + 0.5;
  frag_colour = vec4(value, value, value, 1.0);
  return;
}

// Define right optomotor gratings
void right_optomotor_gratings()
{
  float value = 0.5 * sin(((tex_coord.x - fish_position_x) * sin(fish_heading_angle + halfpi) + (tex_coord.y - fish_position_y) * cos(fish_heading_angle + halfpi)) * twopi * optomotor_gratings_spat_freq - optomotor_gratings_speed * time) + 0.5;
  frag_colour = vec4(value, value, value, 1.0);
  return;
}

// Define converging gratings
void converging_gratings()
{
  float value;
  if (tex_coord.x < fish_position_x)
  {
    value = 0.5 * sin(tex_coord.x * twopi * optomotor_gratings_spat_freq - optomotor_gratings_speed * time) + 0.5;
  }
  else
  {
    value = 0.5 * sin(tex_coord.x * twopi * optomotor_gratings_spat_freq + optomotor_gratings_speed * time) + 0.5;
  }
  frag_colour = vec4(value, value, value, 1.0);
  return;
}

// Define diverging gratings
void diverging_gratings()
{
  float value;
  if (tex_coord.x < fish_position_x)
  {
    value = 0.5 * sin(tex_coord.x * twopi * optomotor_gratings_spat_freq + optomotor_gratings_speed * time) + 0.5;
  }
  else
  {
    value = 0.5 * sin(tex_coord.x * twopi * optomotor_gratings_spat_freq - optomotor_gratings_speed * time) + 0.5;
  }
  frag_colour = vec4(value, value, value, 1.0);
  return;
}

// Define concentric gratings
void concentric_gratings()
{
  float radius = sqrt(pow(tex_coord.x - 0.5, 2) + pow(tex_coord.y - 0.5, 2));
  float value = 0.5 * sin(radius * twopi * optomotor_gratings_spat_freq + optomotor_gratings_speed * time) + 0.5;
  frag_colour = vec4(value, value, value, 1.0);
  return;
}

// Define left optokinetic gratings
void left_optokinetic_gratings()
{
  float radius = sqrt(pow(tex_coord.x - fish_position_x, 2) + pow(tex_coord.y - fish_position_y, 2));
  if (radius < optokinetic_gratings_inner_rad || radius > optokinetic_gratings_outer_rad)
  {
    frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
  else
  {
    float angle = atan((tex_coord.x - fish_position_x) * sin(fish_heading_angle) + (tex_coord.y - fish_position_y) * cos(fish_heading_angle), (tex_coord.x - fish_position_x) * cos(fish_heading_angle) - (tex_coord.y - fish_position_y) * sin(fish_heading_angle));
    float value = 0.5 * sin(angle * twopi * optokinetic_gratings_spat_freq - optokinetic_gratings_speed * time) + 0.5;
    frag_colour = vec4(value, value, value, 1.0);
  }
  return;
}

// Define right optokinetic gratings
void right_optokinetic_gratings()
{
  float radius = sqrt(pow(tex_coord.x - fish_position_x, 2) + pow(tex_coord.y - fish_position_y, 2));
  if (radius < optokinetic_gratings_inner_rad || radius > optokinetic_gratings_outer_rad)
  {
    frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
  else
  {
    float angle = atan((tex_coord.x - fish_position_x) * sin(fish_heading_angle) + (tex_coord.y - fish_position_y) * cos(fish_heading_angle), (tex_coord.x - fish_position_x) * cos(fish_heading_angle) - (tex_coord.y - fish_position_y) * sin(fish_heading_angle));
    float value = 0.5 * sin(angle * twopi * optokinetic_gratings_spat_freq + optokinetic_gratings_speed * time) + 0.5;
    frag_colour = vec4(value, value, value, 1.0);
  }
  return;
}

// Define forward moving prey
void forward_moving_prey()
{
  vec2 circle_position = vec2(fish_position_x + moving_prey_dist * sin(fish_heading_angle - thirdpi * sin(time * moving_prey_speed)), fish_position_y + moving_prey_dist * cos(fish_heading_angle - thirdpi * sin(time * moving_prey_speed)));
  if (sqrt(pow(tex_coord.x - circle_position[0], 2) + pow(tex_coord.y - circle_position[1], 2)) < moving_prey_size)
  {
    frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  }
  else
  {
    frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
  return;
}

// Define left moving prey
void left_moving_prey()
{
  vec2 circle_position = vec2((fish_position_x + moving_prey_dist * sin(fish_heading_angle - (halfpi * (0.5 * sin(time * moving_prey_speed) + 1.0)))), (fish_position_y + moving_prey_dist * cos(fish_heading_angle - (halfpi * (0.5 * sin(time * moving_prey_speed) + 1.0)))));
  if (sqrt(pow(((tex_coord.x - circle_position[0])), 2) + pow((tex_coord.y - circle_position[1]), 2)) < moving_prey_size)
  {
    frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  }
  else
  {
    frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
  return;
}

// Define right moving prey
void right_moving_prey()
{
  vec2 circle_position = vec2((fish_position_x + moving_prey_dist * sin(fish_heading_angle + (halfpi * (0.5 * sin(time * moving_prey_speed) + 1.0)))), (fish_position_y + moving_prey_dist * cos(fish_heading_angle + (halfpi * (0.5 * sin(time * moving_prey_speed) + 1.0)))));
  if (sqrt(pow(((tex_coord.x - circle_position[0])), 2) + pow((tex_coord.y - circle_position[1]), 2)) < moving_prey_size)
  {
    frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  }
  else
  {
    frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
  return;
}

// Define left stationary prey
void left_stationary_prey()
{
  vec2 circle_position = vec2(fish_position_x + stationary_prey_dist * sin(fish_heading_angle - thirdpi), fish_position_y + stationary_prey_dist * cos(fish_heading_angle - thirdpi));
  if (sqrt(pow(tex_coord.x - circle_position[0], 2) + pow(tex_coord.y - circle_position[1], 2)) < stationary_prey_size)
  {
    frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  }
  else
  {
      frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
  return;
}

// Define right stationary prey
void right_stationary_prey()
{
  vec2 circle_position = vec2(fish_position_x + stationary_prey_dist * sin(fish_heading_angle + thirdpi), fish_position_y + stationary_prey_dist * cos(fish_heading_angle + thirdpi));
  if (sqrt(pow(tex_coord.x - circle_position[0], 2) + pow(tex_coord.y - circle_position[1], 2)) < stationary_prey_size)
  {
    frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  }
  else
  {
      frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  }
  return;
}

void main()
{
  switch (stimulus_number)
  {
    case 0:
      solid_black();
      break;
    case 1:
      solid_white();
      break;
    case 2:
      black_white_flashes();
      break;
    case 3:
      left_phototaxis();
      break;
    case 4:
      right_phototaxis();
      break;
    case 5:
      left_looming_dot();
      break;
    case 6:
      right_looming_dot();
      break;
    case 7:
      left_optomotor_gratings();
      break;
    case 8:
      right_optomotor_gratings();
      break;
    case 9:
      converging_gratings();
      break;
    case 10:
      diverging_gratings();
      break;
    case 11:
      concentric_gratings();
      break;
    case 12:
      left_optokinetic_gratings();
      break;
    case 13:
      right_optokinetic_gratings();
      break;
    case 14:
      forward_moving_prey();
      break;
    case 15:
      left_moving_prey();
      break;
    case 16:
      right_moving_prey();
      break;
    case 17:
      left_stationary_prey();
      break;
    case 18:
      right_stationary_prey();
      break;
  }
}
