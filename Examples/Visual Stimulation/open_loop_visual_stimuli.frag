#version 400

// Define uniform variables incoming from Bonsai
uniform float fish_position_x;
uniform float fish_position_y;
uniform float fish_heading_angle;
uniform float time;
uniform int switch_stim;

// Define OpenGL incoming and outgoing variables
in vec2 tex_coord;
out vec4 frag_colour;

// Define useful variable constants
const float quarterpi = 0.78539816339;
const float thirdpi = 1.0471975512;
const float halfpi = 1.57079632679;
const float pi = 3.14159265359;
const float threehalvespi = 4.71238898038;
const float twopi = 6.28318530718;
const float inf = 1. / 0.;

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

// Define solid black
void solid_black_stimulus()
{
  frag_colour = vec4(0.0, 0.0, 0.0, 1.0);
  return;
}

// Define solid white
void solid_white_stimulus()
{
  frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
  return;
}

// Define leftside looming dot
void leftside_looming_dot()
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

// Define rightside looming dot
void rightside_looming_dot()
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

// Define leftward moving gratings
void leftward_optomotor_gratings()
{
  float value = 0.5 * sin(((tex_coord.x - fish_position_x) * sin(fish_heading_angle + threehalvespi) + (tex_coord.y - fish_position_y) * cos(fish_heading_angle + threehalvespi)) * twopi * optomotor_gratings_spat_freq - optomotor_gratings_speed * time) + 0.5;
  frag_colour = vec4(value, value, value, 1.0);
  return;
}

// Define rightward moving gratings
void rightward_optomotor_gratings()
{
  float value = 0.5 * sin(((tex_coord.x - fish_position_x) * sin(fish_heading_angle + halfpi) + (tex_coord.y - fish_position_y) * cos(fish_heading_angle + halfpi)) * twopi * optomotor_gratings_spat_freq - optomotor_gratings_speed * time) + 0.5;
  frag_colour = vec4(value, value, value, 1.0);
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

// Define leftside moving prey
void leftside_moving_prey()
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

// Define rightside moving prey
void rightside_moving_prey()
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

// Define leftside stationary prey
void leftside_stationary_prey()
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

// Define rightside stationary prey
void rightside_stationary_prey()
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

// Define concentric gratings
void concentric_gratings()
{
  float radius = sqrt(pow(tex_coord.x - 0.5, 2) + pow(tex_coord.y - 0.5, 2));
  float value = 0.5 * sin(radius * twopi * optomotor_gratings_spat_freq + optomotor_gratings_speed * time) + 0.5;
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

// Define leftward phototaxic stimulus
void leftward_phototaxic()
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

// Define rightward phototaxic stimulus
void rightward_phototaxic()
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

// Define leftward moving gratings
void leftward_optokinetic_gratings()
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

// Define rightward moving gratings
void rightward_optokinetic_gratings()
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

void main()
{
  switch (switch_stim)
  {
    case 0:
      solid_black_stimulus();
      break;
    case 1:
      solid_white_stimulus();
      break;
    case 2:
      leftside_looming_dot();
      break;
    case 3:
      rightside_looming_dot();
      break;
    case 4:
      leftward_optomotor_gratings();
      break;
    case 5:
      rightward_optomotor_gratings();
      break;
    case 6:
      forward_moving_prey();
      break;
    case 7:
      leftside_moving_prey();
      break;
    case 8:
      rightside_moving_prey();
      break;
    case 9:
      leftside_stationary_prey();
      break;
    case 10:
      rightside_stationary_prey();
      break;
    case 11:
      concentric_gratings();
      break;
    case 12:
      converging_gratings();
      break;
    case 13:
      leftward_phototaxic();
      break;
    case 14:
      rightward_phototaxic();
      break;
    case 15:
      leftward_optokinetic_gratings();
      break;
    case 16:
      rightward_optokinetic_gratings();
      break;
  }
}
