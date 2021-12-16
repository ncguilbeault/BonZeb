import matplotlib
from matplotlib.patches import Rectangle, Patch
import numpy as np
import matplotlib.pyplot as plt
import csv
import cv2
import os
from matplotlib.colors import LinearSegmentedColormap
import scipy.stats as stats
import seaborn as sns
from matplotlib import cm
from scipy.optimize import linear_sum_assignment
from scipy.spatial.distance import cdist
import subprocess

def filenames_from_folder(folder, filename_starts_with = None, filename_contains = None, filename_ends_with = None, filename_does_not_contain = None):
    '''
    Function that returns the filenames contained inside a folder.
    The function can be provided with arguments to specify which files to look for. This includes what the filenames start and end with, as well as if something is contained in the filename.
    '''
    # Return the names of all the files in the folder
    filenames = ['{0}\{1}'.format(folder, os.path.basename(filename)) for filename in os.listdir(folder)]
    # Check if filename_starts_with was given
    if filename_starts_with != None:
        # Return the names of all the files that start with ...
        filenames = ['{0}\{1}'.format(folder, os.path.basename(filename)) for filename in filenames if filename.startswith(filename_starts_with)]
    # Check if filename_contains was given
    if filename_contains != None:
        if isinstance(filename_contains, list):
            for item in filename_contains:
                filenames = ['{0}\{1}'.format(folder, os.path.basename(filename)) for filename in filenames if item in filename]
        else:
            # Return the names of all the files that contain ...
            filenames = ['{0}\{1}'.format(folder, os.path.basename(filename)) for filename in filenames if filename_contains in filename]
    # Check if filename_ends_with was given
    if filename_ends_with != None:
        # Return the names of all the files that end with ...
        filenames = ['{0}\{1}'.format(folder, os.path.basename(filename)) for filename in filenames if filename.endswith(filename_ends_with)]
    # Check if filename_does_not_contain was given
    if filename_does_not_contain != None:
        if isinstance(filename_does_not_contain, list):
            for item in filename_does_not_contain:
                filenames = ['{0}\{1}'.format(folder, os.path.basename(filename)) for filename in filenames if item not in os.path.basename(filename)]
        else:
            # Return the names of all the files that do not contain ...
            filenames = ['{0}\{1}'.format(folder, os.path.basename(filename)) for filename in filenames if filename_does_not_contain not in filename]
    return filenames

def convert_video(video_filename):
    ffmpeg_command = f"ffmpeg -y -i \"{video_filename}\" -vcodec mjpeg \"{video_filename[:-4]}_converted.avi\""
    return subprocess.call(ffmpeg_command)

def extract_tracking_points_from_csv(csv_file):
    with open(csv_file) as f:
        tracking_points = [[column for column in row if len(column) > 0] for row in csv.reader(f)]
    tracking_points = np.array([np.array([np.array([point.split(', ') for point in fish_data[3:-3].split(")', '(")]).astype(float) for fish_data in data_point]) for data_point in tracking_points])
    return tracking_points

def extract_tail_curvature_from_csv(csv_file):
    with open(csv_file) as f:
        tail_curvature = [[column for column in row if len(column) > 0] for row in csv.reader(f)]
    tail_curvature = np.array([np.array([fish_data[1:-1].split(", ") for fish_data in data_point]).astype(float) for data_point in tail_curvature])
    return tail_curvature

def reorder_data_for_identities_tail_curvature(tracking_data, tail_curvature, n_fish, n_tracking_points):
    new_tracking_data = np.zeros((len(tracking_data), n_fish, n_tracking_points, 2))
    new_tail_curvature = np.zeros((len(tail_curvature), n_fish, n_tracking_points - 1))
    for i in range(len(tracking_data) - 1):
        if i == 0:
            first_tracking_data_arr = tracking_data[0]
            new_tracking_data[0] = tracking_data[0]
            new_tail_curvature[0] = tail_curvature[0]
        second_tracking_data_arr = tracking_data[i + 1]
        tail_curvature_arr = tail_curvature[i + 1]
        new_tracking_data_arr, new_tail_curvature_arr, new_order = find_identities_tail_curvature(first_tracking_data_arr, second_tracking_data_arr, tail_curvature_arr, n_fish, n_tracking_points)
        new_tracking_data[i + 1] = new_tracking_data_arr[new_order[1]]
        new_tail_curvature[i + 1] = new_tail_curvature_arr[new_order[1]]
        first_tracking_data_arr = new_tracking_data[i + 1]
    return [new_tracking_data, new_tail_curvature]

def find_identities_tail_curvature(first_tracking_data_arr, second_tracking_data_arr, tail_curvature_arr, n_fish, n_tracking_points):
    cost = cdist(first_tracking_data_arr[:, 0], second_tracking_data_arr[:, 0])
    result = linear_sum_assignment(cost)
    if second_tracking_data_arr.shape[0] < n_fish:
        missed_index = [i for i in range(len(first_tracking_data_arr)) if i not in result[0]][0]
        merged_index = np.where(cost[missed_index] == np.min(cost[missed_index]))[0][0]
        second_tracking_data_arr = np.append(second_tracking_data_arr, second_tracking_data_arr[merged_index]).reshape(-1, n_tracking_points, 2)
        tail_curvature_arr = np.append(tail_curvature_arr, tail_curvature_arr[merged_index]).reshape(-1, n_tracking_points - 1)
        second_tracking_data_arr, tail_curvature_arr, result = find_identities_tail_curvature(first_tracking_data_arr, second_tracking_data_arr, tail_curvature_arr, n_fish, n_tracking_points)
    return second_tracking_data_arr, tail_curvature_arr, result

def reorder_data_for_identities_tail_points(tracking_data, n_fish, n_tracking_points, start_index = 0):
    new_tracking_data = np.zeros((len(tracking_data), n_fish, n_tracking_points, 2))
    for i in range(len(tracking_data) - 1):
        if i == 0:
            first_tracking_data_arr = tracking_data[start_index]
            new_tracking_data[0] = tracking_data[start_index]
        second_tracking_data_arr = tracking_data[i + 1]
        new_tracking_data_arr, new_order = find_identities_tail_points(first_tracking_data_arr, second_tracking_data_arr, n_fish, n_tracking_points)
        new_tracking_data[i + 1] = new_tracking_data_arr[new_order[1]]
        first_tracking_data_arr = new_tracking_data[i + 1]
    return new_tracking_data

def find_identities_tail_points(first_tracking_data_arr, second_tracking_data_arr, n_fish, n_tracking_points):
    cost = cdist(first_tracking_data_arr[:, 0], second_tracking_data_arr[:, 0])
    result = linear_sum_assignment(cost)
    if second_tracking_data_arr.shape[0] < n_fish:
        missed_index = [i for i in range(len(first_tracking_data_arr)) if i not in result[0]][0]
        merged_index = np.where(cost[missed_index] == np.min(cost[missed_index]))[0][0]
        second_tracking_data_arr = np.append(second_tracking_data_arr, second_tracking_data_arr[merged_index]).reshape(-1, n_tracking_points, 2)
        second_tracking_data_arr, result = find_identities_tail_points(first_tracking_data_arr, second_tracking_data_arr, n_fish, n_tracking_points)
    return second_tracking_data_arr, result

def find_tracking_errors(tracking_data, window = None):
    tracking_errors = np.zeros((tracking_data.shape[:2]))
    if window is None:
        for time_index, fish_data in enumerate(tracking_data):
            dupes = np.unique(fish_data[:, 0], axis = 0, return_counts = True)
            dupe_vals = dupes[0][dupes[1] > 1]
            for fish_index, fish_val in enumerate(fish_data[:, 0]):
                for dupe_val in dupe_vals:
                    if np.array_equal(fish_val, dupe_val):
                        tracking_errors[time_index, fish_index] = 1
    else:
        for time_index, fish_data in enumerate(tracking_data[int(window / 2) : -int(window / 2)]):
            dupes = np.unique(fish_data[:, 0], axis = 0, return_counts = True)
            dupe_vals = dupes[0][dupes[1] > 1]
            for fish_index, fish_val in enumerate(fish_data[:, 0]):
                for dupe_val in dupe_vals:
                    if np.array_equal(fish_val, dupe_val):
                        tracking_errors[time_index : time_index + int(window), fish_index] = 1
    return tracking_errors

def remove_tracking_errors_from_tail_curvature(tail_curvature, tracking_errors):
    processed_tail_curvature = tail_curvature.copy()
    processed_tail_curvature[tracking_errors == 1] = 0
    return processed_tail_curvature

def load_tracking_data(folder, prefix, n_fish, n_tracking_points):
    tracking_data_csv = filenames_from_folder(folder, filename_contains = [prefix, "tracking-results"], filename_ends_with = ".csv")[0]
    tail_curvature_csv = filenames_from_folder(folder, filename_contains = [prefix, "tail-curvature"], filename_ends_with = ".csv")[0]
    tracking_data = extract_tracking_points_from_csv(tracking_data_csv)
    tail_curvature = extract_tail_curvature_from_csv(tail_curvature_csv)
    tracking_data, tail_curvature = reorder_data_for_identities(tracking_data, tail_curvature, n_fish, n_tracking_points)
    return tracking_data, tail_curvature

def load_image(folder, prefix, example_index):
    video_path = filenames_from_folder(folder, filename_contains = [prefix, "video"], filename_ends_with = ".avi")[0]
    cap = cv2.VideoCapture(video_path)
    cap.set(cv2.CAP_PROP_POS_FRAMES, example_index)
    image = cap.read()[1]
    cap.release()
    return image

def plot_image_with_tracking_overlay(tracking_data, image, save_path = None, example_index = 0, index_factor = 1):
    tracking_colours = np.array([cm.get_cmap("hsv")(plt.Normalize(0, tracking_data.shape[1])(i)) for i in range(tracking_data.shape[1])])
    new_image = image.copy()
    for fish_index, fish_tracking_points in enumerate(tracking_data[int(example_index * index_factor)]):
        print("Fish {0} - Colour {1}".format(fish_index + 1, tracking_colours[fish_index]*255))
        cv2.circle(new_image, (int(float(fish_tracking_points[0, 0])), int(float(fish_tracking_points[0,1]))), 3, tracking_colours[fish_index] * 255, 1, cv2.LINE_AA)
        cv2.putText(new_image, "Fish {0}".format(fish_index + 1), (int(float(fish_tracking_points[0, 0])) + 10, int(float(fish_tracking_points[0,1])) + 10), cv2.FONT_HERSHEY_SIMPLEX, 1, tracking_colours[fish_index] * 255)
    fig = plt.figure(figsize = (10, 10), dpi = 300, constrained_layout = False)
    im_ax = fig.add_subplot(1, 1, 1)
    im_ax.imshow(new_image, aspect = "equal")
    im_ax.set_axis_off()

    if save_path is not None:
        plt.savefig(save_path)
    plt.show()

def calculate_paths(tracking_data, paths, linewidth):
    for time_index in range(tracking_data.shape[0] - 1):
        for fish_index in range(tracking_data[time_index].shape[0]):
            point1 = (int(tracking_data[time_index, fish_index, 0, 0]), int(tracking_data[time_index, fish_index, 0, 1]))
            point2 = (int(tracking_data[time_index + 1, fish_index, 0, 0]), int(tracking_data[time_index + 1, fish_index, 0, 1]))
            if point1 != point2:
                paths[fish_index] = cv2.line(paths[fish_index], point1, point2, (time_index + 1) / tracking_data.shape[0] * 255, linewidth)
    return paths

def plot_paths(tracking_data, image, linewidth = 1, save_path = None):
    tracking_colours = np.array([cm.get_cmap("hsv")(plt.Normalize(0, tracking_data.shape[1])(i)) for i in range(tracking_data.shape[1])])
    path_colours = [LinearSegmentedColormap.from_list("cmap_{0}".format(fish_index + 1), [[np.min([1, tracking_colour[0] * 1.6 + 0.8]), np.min([1, tracking_colour[1] * 1.6 + 0.8]), np.min([1, tracking_colour[2] * 1.6 + 0.8]), 1], tracking_colour, [np.max([0, tracking_colour[0] * 0.6 - 0.2]), np.max([0, tracking_colour[1] * 0.6 - 0.2]), np.max([0, tracking_colour[2] * 0.6 - 0.2]), 1]]) for fish_index, tracking_colour in enumerate(tracking_colours)]
    [path_colour.set_under(color = [1, 1, 1, 0]) for path_colour in path_colours]
    paths = np.zeros((tracking_colours.shape[0], image.shape[0], image.shape[1]))
    paths = calculate_paths(tracking_data, paths, linewidth)

    fig = plt.figure(figsize = (10, 10), dpi = 300, constrained_layout = False)
    path_axes = [fig.add_subplot(1, 1, 1, label = "{0}".format(index)) for index, path in enumerate(paths)]
    [path_ax.imshow(path, cmap = path_colour, origin = "upper", vmin = 0.000000000001, vmax = 255, aspect = "equal") for path_ax, path, path_colour in zip(path_axes, paths, path_colours)]
    [path_ax.set_facecolor([1, 1, 1, 0]) for path_ax in path_axes]
    [path_ax.set_axis_off() for path_ax in path_axes]

    if save_path is not None:
        plt.savefig(save_path)
    plt.show()

def plot_colorbar(tracking_data, save_path = None):
    colorbar_data = np.linspace(0, 255, tracking_data.shape[0])[:, np.newaxis]
    fig = plt.figure(figsize = (0.5, 10), dpi = 300, constrained_layout = False)
    colorbar_ax = fig.add_subplot(1, 1, 1)
    colorbar_ax.imshow(colorbar_data, cmap = "gray", aspect = "auto")
    colorbar_ax.set_axis_off()
    if save_path is not None:
        plt.savefig(save_path)
    plt.show()

def plot_tail_curvature(tail_curvature, save_path = None, imaging_FPS = 332, tracking_errors = None):

    fig = plt.figure(figsize = (30, 5), dpi = 300, constrained_layout = False)

    gs = fig.add_gridspec(ncols = 1, nrows = tail_curvature.shape[1], hspace = -0.3)

    tc_axes = [fig.add_subplot(gs[i, 0]) for i in range(tail_curvature.shape[1])]
    x_vals = np.linspace(0, tail_curvature.shape[0]/imaging_FPS, tail_curvature.shape[0])
    y_vals_baseline = np.zeros((x_vals.shape[0]))

    tracking_colours = np.array([cm.get_cmap("hsv")(plt.Normalize(0, tail_curvature.shape[1])(i)) for i in range(tracking_data.shape[1])])

    for tc_index, tc_ax in enumerate(tc_axes):
        tc_ax.plot(x_vals, np.mean(tail_curvature[:, tc_index, -3:], axis = 1), color = tracking_colours[tc_index], linewidth = 1, rasterized = True)
        tc_ax.plot(x_vals, y_vals_baseline, color = tracking_colours[tc_index], linewidth = 1, rasterized = True, alpha = 0.5, ls = "--")
        tc_ax.set_ylim(-150, 150)
        tc_ax.set_xlim(-0.01, x_vals[-1])
        tc_ax.spines['top'].set_visible(False)
        tc_ax.spines['right'].set_visible(False)
        tc_ax.set_yticks([])
        tc_ax.set_xticks([])
        tc_ax.set_facecolor([1, 1, 1, 0])
        if tc_index == len(tc_axes) - 1:
            tc_ax.spines['bottom'].set_bounds(0, 1)
            tc_ax.spines['bottom'].set_linewidth(5)
            tc_ax.spines['left'].set_bounds(0, 100)
            tc_ax.spines['left'].set_linewidth(5)
        else:
            tc_ax.spines['bottom'].set_visible(False)
            tc_ax.spines['left'].set_visible(False)
        if tracking_errors is not None:
            tc_ax.fill_between(x_vals, -150, 150, where = tracking_errors[tc_index] == 1, color = "red", alpha = 0.5, edgecolor = [1, 1, 1, 0])

    # Save the plot and show
    if save_path is not None:
        plt.savefig(save_path)
    plt.show()

def extract_stimulus_data(csv_file):
    with open(csv_file) as f:
        stimulus_data = [[column for column in row if len(column) > 0] for row in csv.reader(f)]
    stimulus_data = np.array([np.array([prey_data.split(",") for prey_data in data_point]).astype(float) for data_point in stimulus_data])
    return stimulus_data

def load_stimulus_data(folder, prefix):
    stimulus_data_csv = filenames_from_folder(folder, filename_contains = [prefix, "stimulus-data"], filename_ends_with = ".csv")[0]
    stimulus_data = extract_stimulus_data(stimulus_data_csv)
    return stimulus_data

def load_data_from_filenames(filenames, dtype = float):
    return [np.loadtxt(filename, dtype = dtype) for filename in filenames]

def calculate_stimulus(stimulus_data, stimulus_image, example_index = 0, index_factor = 1):
    stimulus_sizes = stimulus_data[:, :, 2]
    stimulus_positions = np.moveaxis(np.array([stimulus_data[:, :, 0] * image.shape[0] / 2 + image.shape[0] / 2, stimulus_data[:, :, 1] * image.shape[1] / 2 + image.shape[1] / 2]), 0, -1)
    for stimulus_position, stimulus_size in zip(stimulus_positions[example_index * index_factor], stimulus_sizes[example_index * index_factor]):
        stimulus_image = cv2.circle(stimulus_image, (int(stimulus_position[0]), int(stimulus_position[1])), int(stimulus_size), [255, 255, 255], -1, cv2.LINE_AA)
    return stimulus_image

def plot_stimulus(stimulus_data, stimulus_image, save_path = None, example_index = 0, index_factor = 1):

    stimulus_image = calculate_stimulus(stimulus_data, stimulus_image, example_index, index_factor)

    fig = plt.figure(figsize = (10, 10), dpi = 300, constrained_layout = False)
    im_ax = fig.add_subplot(1, 1, 1)
    im_ax.imshow(stimulus_image, cmap = "gray", vmin = 0, vmax = 255, aspect = "equal")
    im_ax.set_axis_off()

    if save_path is not None:
        plt.savefig(save_path)
    plt.show()

def interpolate_NaNs(data, skip_start = False, skip_end = False):
    if not skip_start and np.isnan(data[0]):
        data[0] = 0
    if not skip_end and np.isnan(data[-1]):
        data[-1] = 0
    if np.isnan(data).any():
        nans = np.isnan(data)
        nan_indices = np.where(nans)[0]
        good_indices = np.where(nans == False)[0]
        data[nan_indices] = np.interp(nan_indices, good_indices, data[good_indices])
    return data

def calculate_prey_yaw_angle(time, moving_prey_speed = 0.6):
    return [np.arctan2(np.sin((-np.pi / 3) * np.sin(t * moving_prey_speed)), np.cos((-np.pi / 3) * np.sin(t * moving_prey_speed))) for t in time]

def calculate_velocity(pos_x, pos_y, size_of_FOV_cm = 6, image_width = 1088, imaging_FPS = 332):
    return [np.hypot(np.diff(x[:np.min([len(x), len(y)])]), np.diff(y[:np.min([len(x), len(y)])])) * size_of_FOV_cm * imaging_FPS / image_width for x, y in zip(pos_x, pos_y)]

def calculate_paths2(pos_x, pos_y, image, colour, linewidth = 1, radius_threshold = 100):
    for x, y in zip(pos_x, pos_y):
        for i in range(len(x) - 1):
            point1 = (int(x[i]), int(y[i]))
            point2 = (int(x[i+1]), int(y[i+1]))
            if np.sqrt((point2[0] - (image.shape[0] / 2))**2 + (point2[1]  - (image.shape[1] / 2))**2) > radius_threshold:
                continue
            if point1 != point2:
                image = cv2.line(image, point1, point2, colour, linewidth)
    return image

def calculate_trajectory(x, y, image, colour, linewidth = 1, radius_threshold = 100):
    for i in range(len(x) - 1):
        point1 = (int(x[i]), int(y[i]))
        point2 = (int(x[i+1]), int(y[i+1]))
        if np.sqrt((point2[0] - (image.shape[0] / 2))**2 + (point2[1]  - (image.shape[1] / 2))**2) > radius_threshold:
            continue
        if point1 != point2:
            image = cv2.line(image, point1, point2, colour, linewidth)
    return image

def threshold_trajectory(pos_x, pos_y, image, radius_threshold = 100):
    thresh = [np.ones(x.shape).astype(bool) for x in pos_x]
    for i, (x, y) in enumerate(zip(pos_x, pos_y)):
        for j in range(len(x) - 1):
            point1 = (int(x[j]), int(y[j]))
            point2 = (int(x[j+1]), int(y[j+1]))
            if np.sqrt((point2[0] - (image.shape[0] / 2))**2 + (point2[1]  - (image.shape[1] / 2))**2) > radius_threshold:
                thresh[i][j:] = False
                break
    return thresh

def normalize_path_data(pos_x, pos_y, heading_angle, offset_x = 200, offset_y = 200):
    new_pos_x = []
    new_pos_y = []
    for x, y, ha in zip(pos_x, pos_y, heading_angle):
        x = x - x[0]
        y = y - y[0]
        xnew = (x * np.cos(ha[0] + (np.pi/2)) - y * np.sin(ha[0] + (np.pi/2))) + offset_x
        ynew = (x * np.sin(ha[0] + (np.pi/2)) + y * np.cos(ha[0] + (np.pi/2))) + offset_y
        new_pos_x.append(xnew)
        new_pos_y.append(ynew)
    return new_pos_x, new_pos_y

def calculate_max_heading_angle(heading_angle):
    max_heading_angle = []
    for ha in heading_angle:
        max_i = np.argmax(ha)
        min_i = np.argmin(ha)
        if np.abs(ha[max_i]) < np.abs(ha[min_i]):
            max_heading_angle.append(ha[min_i])
        else:
            max_heading_angle.append(ha[max_i])
    return max_heading_angle

def detect_peaks(data, delta, x = None):
    maxtab = []
    mintab = []
    if x is None:
        x = np.arange(len(data))
    data = np.asarray(data)
    if len(data) != len(x):
        sys.exit('Input vectors data and x must have same length')
    if not np.isscalar(delta):
        sys.exit('Input argument delta must be a scalar')
    if delta <= 0:
        sys.exit('Input argument delta must be positive')
    mn, mx = np.Inf, -np.Inf
    mnpos, mxpos = np.NaN, np.NaN
    lookformax = True
    for i in np.arange(len(data)):
        this = data[i]
        if this > mx:
            mx = this
            mxpos = x[i]
        if this < mn:
            mn = this
            mnpos = x[i]
        if lookformax:
            if this < mx-delta:
                maxtab.append((mxpos, mx))
                mn = this
                mnpos = x[i]
                lookformax = False
        else:
            if this > mn+delta:
                mintab.append((mnpos, mn))
                mx = this
                mxpos = x[i]
                lookformax = True
    maxtab = np.array(maxtab)
    mintab = np.array(mintab)
    if len(maxtab) > 0 and len(mintab) > 0:
        results = np.array(sorted(np.concatenate([maxtab, mintab], axis = 0), key = lambda x: x[0]))
    else:
        results = np.ones(2) * np.nan
    return results

def detect_peaks_2(data, bout_threshold):
    min_value = np.Inf
    max_value = -np.Inf
    find_max = True
    peaks = []
    bout_counter = 0
    bout = False
    for index, value in enumerate(data):
        if value > max_value:
            max_value = value
            if len(peaks) > 0:
                if not find_max and peaks[-1] == index - 1:
                    peaks[-1] = index
        if value < min_value:
            min_value = value
            if len(peaks) > 0:
                if find_max and peaks[-1] == index - 1:
                    peaks[-1] = index
        if bout:
            if find_max and value > min_value + bout_threshold:
                max_value = value
                find_max = False
                peaks.append(index)
            elif not find_max and value < max_value - bout_threshold:
                min_value = value
                find_max = True
                peaks.append(index)
        else:
            if value > min_value + bout_threshold or value < max_value - bout_threshold:
                bout = True
                if value < max_value - bout_threshold:
                    find_max = False
    if len(peaks) > 0:
        return peaks
    else:
        return np.nan

def detect_bouts(data, bout_threshold, window_size):
    min_value = np.Inf
    max_value = -np.Inf
    find_max = True
    bout_detected = np.zeros(len(data)).astype(bool)
    bout_counter = 0
    prev_value = None
    bout = False
    for index, value in enumerate(data):
        if value > max_value:
            max_value = value
        if value < min_value:
            min_value = value
        if bout:
            if not find_max and value > min_value + bout_threshold:
                max_value = value
                if not find_max:
                    find_max = True
            elif find_max and value < max_value - bout_threshold:
                min_value = value
                if find_max:
                    find_max = False
            if np.abs(value - prev_value) > bout_threshold:
                bout_counter = 0
            else:
                bout_counter += 1
        else:
            if value > min_value + bout_threshold or value < max_value - bout_threshold:
                bout = True
                bout_counter = 0
                if value < max_value - bout_threshold:
                    find_max = False
        if bout_counter > window_size:
            min_value = np.Inf
            max_value = -np.Inf
            bout = False
            find_max = True
            bout_counter = 0
        prev_value = value
        bout_detected[index] = bout
    return bout_detected

def calculate_frequency_from_peaks(data, data_length, framerate):
    new_data = np.zeros(data_length)
    for peak0, peak1 in zip(data[:-1], data[1:]):
         frequency = framerate / (2 * (peak1 - peak0))
         new_data[peak0:peak1] = frequency
    new_data[-1] = frequency
    return new_data

def calculate_zscore(data):
    return (data - np.mean(data)) / np.std(data)

def find_outlier_indices(data, zscore_threshold = np.Inf, max_val_threshold = np.Inf, min_val_threshold = -np.Inf):
    zscore = calculate_zscore(data)
    max_vals = data > max_val_threshold
    min_vals = data < min_val_threshold
    zscore_vals = zscore > zscore_threshold
    outlier_indices = np.where(np.logical_or.reduce([zscore_vals, max_vals, min_vals]))[0]
    return outlier_indices

def plot_tbf_heatmap(frequency, save_path = None):

    fig = plt.figure(figsize = (20, 8), dpi = 300, constrained_layout = False)

    gs = fig.add_gridspec(ncols = 1, nrows = 2, height_ratios= [1, 0.02], hspace = 0)
    hm_ax = fig.add_subplot(gs[0, 0])
    sc_ax = fig.add_subplot(gs[1, 0])

    hm_ax.imshow(frequency, origin = 'upper', aspect = 'auto', interpolation = "none", vmin = 0, vmax = 40, cmap = 'inferno', rasterized = True)
    hm_ax.set_axis_off()
    sc_ax.set_xlim(0, frequency.shape[1])
    sc_ax.set_xticks([])
    sc_ax.set_yticks([])
    sc_ax.spines['top'].set_visible(False)
    sc_ax.spines['right'].set_visible(False)
    sc_ax.spines['left'].set_visible(False)
    sc_ax.spines['bottom'].set_visible(False)
    # sc_ax.spines['bottom'].set_bounds(0, 10 * imaging_FPS)
    # sc_ax.spines['bottom'].set_linewidth(5)

    # Save the plot and show
    if save_path is not None:
        plt.savefig(save_path)
    plt.show()

def custom_smooth_data(data, thresh):
    new_data = data.copy()
    for index, (val1, val2, val3) in enumerate(zip(data[:-2], data[1:-1], data[2:])):
        if np.abs(val2 - np.mean([val1, val3])) > thresh:
            new_data[index + 1] = np.nan
    # filter = np.array([1, 10, 1])
    # filter2 = np.array([1, 10, 10, 1])
    # indices = np.abs(np.convolve(data, filter / np.sum(filter), mode = "same")) + np.abs(np.convolve(data, filter2 / np.sum(filter2), mode = "same")) > thresh
    # new_data[indices] = np.nan
    return interpolate_NaNs(new_data)

def load_timestamped_data(file, timestamp_index = 0, format = "ms", skip_first_row = False):
    # try:
    data = np.genfromtxt(file, dtype = str)
    n_indices = len(data[0].split(","))
    if timestamp_index >= n_indices:
        print("Error! Timestamp index is greater than the number of columns in the data.")
        raise Exception
    if timestamp_index == -1:
        timestamp_index = n_indices - 1
    if skip_first_row:
        timestamps = np.array([val.split(",")[timestamp_index].split("T")[1].split("-")[0].split(":") for val in data[1:]]).astype(float)
    else:
        timestamps = np.array([val.split(",")[timestamp_index].split("T")[1].split("-")[0].split(":") for val in data]).astype(float)
    value_indices = [index for index in range(n_indices) if index != timestamp_index]
    if skip_first_row:
        values = np.array([np.array(val.split(","))[value_indices] for val in data[1:]])
    else:
        values = np.array([np.array(val.split(","))[value_indices] for val in data])
    return convert_timestamps(timestamps, format), values
    # except:
    #     print("Error processing file: {0}".format(file))
    #     return

def convert_timestamps(timestamps, format = "ms"):
    assert format == "ms" or format == "s" or format == "m" or format == "h", "Argument format should be one of the following: ms, s, m, or h."
    if format == "ms":
        timestamps = (timestamps[:,0] * 60 * 60 * 1000) + (timestamps[:,1] * 60 * 1000) + (timestamps[:,2] * 1000)
    if format == "s":
        timestamps = (timestamps[:,0] * 60 * 60) + (timestamps[:,1] * 60) + timestamps[:,2]
    if format == "m":
        timestamps = (timestamps[:,0] * 60) + timestamps[:,1] + (timestamps[:,2] / 60)
    if format == "h":
        timestamps = timestamps[:,0] + (timestamps[:,1] / 60) + (timestamps[:,2] / (60 * 60))
    return timestamps

def normalize_data(data):
    return (data - np.min(data)) / (np.max(data) - np.min(data))

def order_data_by_timestamps(data, timestamps):
    ordered_timestamps = np.unique([val for arr in timestamps for val in arr])
    ordered_data = np.ones((len(data), ordered_timestamps.shape[0])) * np.nan
    for i in range(ordered_data.shape[0]):
        j = 0
        for k in range(ordered_data.shape[1]):
            if timestamps[i][j] <= ordered_timestamps[k]:
                if j == len(timestamps[i]) - 1:
                    ordered_data[i, k:] = data[i][j]
                    break
                else:
                    j += 1
            if j > 0:
                ordered_data[i, k] = data[i][j-1]
    return ordered_data, ordered_timestamps - ordered_timestamps[0]

def order_data_by_timestamps_with_NaNs(data, timestamps):
    ordered_timestamps = np.unique([val for arr in timestamps for val in arr])
    ordered_data = np.ones((len(data), ordered_timestamps.shape[0])) * np.nan
    for i in range(ordered_data.shape[0]):
        j = 0
        for k in range(ordered_data.shape[1]):
            if timestamps[i][j] <= ordered_timestamps[k]:
                if j == len(timestamps[i]) - 1:
                    ordered_data[i, k] = data[i][j]
                    break
                else:
                    ordered_data[i, k] = data[i][j]
                    j += 1
    return ordered_data, ordered_timestamps - ordered_timestamps[0]

# Old 1d closed-loop data analysis
def insert_neg_val_reshape(arr):
    # Insert a negative value into array at fixed intervals and reshape array
    return np.insert(arr, np.arange(4, len(arr) + 1, 4), -1).reshape(-1, 50)

def load_data(folders, exclude = None):
    # Get the list of filenames from each of the folders and sort them based on time of acquiring data
    phase_csvs = [filenames_from_folder(folder, filename_contains = ["_phase"], filename_ends_with = ".csv", filename_does_not_contain = exclude) for folder in folders]
    phase_csvs = [i for list in phase_csvs for i in list]
    phase_csvs = list(sorted(phase_csvs, key = lambda x: [int(x.split("\\")[-1].split("_")[0].split("-")[0]), int(x.split("\\")[-1].split("_")[0].split("-")[1]), int(x.split("\\")[-1].split("_")[0].split("-")[2]), int(x.split("\\")[-1][x.split("\\")[-1].index("fish"):].split("_")[0].split("-")[1])]))
    trial_num_csvs = [filenames_from_folder(folder, filename_contains = ["_trial-num"], filename_ends_with = ".csv", filename_does_not_contain = exclude) for folder in folders]
    trial_num_csvs = [i for list in trial_num_csvs for i in list]
    trial_num_csvs = list(sorted(trial_num_csvs, key = lambda x: [int(x.split("\\")[-1].split("_")[0].split("-")[0]), int(x.split("\\")[-1].split("_")[0].split("-")[1]), int(x.split("\\")[-1].split("_")[0].split("-")[2]), int(x.split("\\")[-1][x.split("\\")[-1].index("fish"):].split("_")[0].split("-")[1])]))
    tail_curvature_csvs = [filenames_from_folder(folder, filename_contains = ["_trial", "tail-curvature"], filename_ends_with = ".csv", filename_does_not_contain = exclude) for folder in folders]
    tail_curvature_csvs = [i for list in tail_curvature_csvs for i in list]
    tail_curvature_csvs = list(sorted(tail_curvature_csvs, key = lambda x: [int(x.split("\\")[-1].split("_")[0].split("-")[0]), int(x.split("\\")[-1].split("_")[0].split("-")[1]), int(x.split("\\")[-1].split("_")[0].split("-")[2]), int(x.split("\\")[-1][x.split("\\")[-1].index("fish"):].split("_")[0].split("-")[1]), int(x.split("\\")[-1][x.split("\\")[-1].index("trial"):].split("_")[0].split("-")[1])]))
    tail_kinematics_csvs = [filenames_from_folder(folder, filename_contains = ["_trial", "tail-kinematics"], filename_ends_with = ".csv", filename_does_not_contain = exclude) for folder in folders]
    tail_kinematics_csvs = [i for list in tail_kinematics_csvs for i in list]
    tail_kinematics_csvs = list(sorted(tail_kinematics_csvs, key = lambda x: [int(x.split("\\")[-1].split("_")[0].split("-")[0]), int(x.split("\\")[-1].split("_")[0].split("-")[1]), int(x.split("\\")[-1].split("_")[0].split("-")[2]), int(x.split("\\")[-1][x.split("\\")[-1].index("fish"):].split("_")[0].split("-")[1]), int(x.split("\\")[-1][x.split("\\")[-1].index("trial"):].split("_")[0].split("-")[1])]))
    # Calculate variables
    phase = np.array([np.diff(np.genfromtxt(phase_csv, delimiter = ",")) for phase_csv in phase_csvs])
    trial_num = np.array([np.genfromtxt(trial_num_csv, delimiter = ",") for trial_num_csv in trial_num_csvs])
    trial_indices = np.array([np.append(np.where(np.diff(trial_num_i))[0], len(trial_num_i) - 1).reshape(-1, 2) for trial_num_i in trial_num])
    tail_curvature = np.array([np.genfromtxt(tail_curvature_csv, delimiter = ",") for tail_curvature_csv in tail_curvature_csvs])
    tail_kinematics = np.array([np.genfromtxt(tail_kinematics_csv, delimiter = ",", dtype = (float, float, bool)) for tail_kinematics_csv in tail_kinematics_csvs])
    frequency = np.array([np.array([value[0] for value in trial_tail_kinematics]) for trial_tail_kinematics in tail_kinematics])
    amplitude = np.array([np.array([value[1] for value in trial_tail_kinematics]) for trial_tail_kinematics in tail_kinematics])
    instance = np.array([np.array([value[2] for value in trial_tail_kinematics]).astype(int) for trial_tail_kinematics in tail_kinematics])
    # Get the order in which gain values were presented based on the gain value specified in the filename
    gain_values = np.array([float(x.split("\\")[-1][x.split("\\")[-1].index("gain"):].split("_")[0].split("-")[1]) for x in tail_curvature_csvs])
    # Insert a value of -1 after every 4 trials to represent the timeout period between successive blocks of trials
    tail_curvature = insert_neg_val_reshape(tail_curvature)
    frequency = insert_neg_val_reshape(frequency)
    amplitude = insert_neg_val_reshape(amplitude)
    instance = insert_neg_val_reshape(instance)
    gain_values = insert_neg_val_reshape(gain_values)
    return [phase_csvs, phase, trial_num, trial_indices, tail_curvature, frequency, amplitude, instance, gain_values]

def calculate_bout_indices_sum_instances_bout_detected(instance, frequency, amplitude, use_amplitude_exclusion = False):
    all_bout_start_indices = [[np.array([]) for j in i] for i in np.zeros(instance.shape)]
    all_bout_end_indices = [[np.array([]) for j in i] for i in np.zeros(instance.shape)]
    sum_instances = np.zeros(instance.shape)
    bout_detected_in_trial = np.zeros(instance.shape)
    for fish_index, (fish_instance, fish_frequency, fish_amplitude) in enumerate(zip(instance, frequency, amplitude)):
        for trial_index, (trial_instance, trial_frequency, trial_amplitude) in enumerate(zip(fish_instance, fish_frequency, fish_amplitude)):
            diff_instances = np.diff(trial_instance.astype(int))
            diff_instances = np.insert(diff_instances, 0, 0)
            bout_start_indices = np.where(diff_instances == 1)[0]
            bout_end_indices = np.where(diff_instances == -1)[0]
            if bout_start_indices.shape[0] > 0 and bout_end_indices.shape[0] > 0:
                if bout_end_indices[0] < bout_start_indices[0]:
                    bout_end_indices = np.delete(bout_end_indices, 0)
                if bout_start_indices.shape[0] == bout_end_indices.shape[0] + 1:
                    bout_start_indices = np.delete(bout_start_indices, bout_start_indices.shape[0] - 1)
                if bout_start_indices.shape[0] != bout_end_indices.shape[0]:
                    print("Error!")
                    continue
                false_bouts = []
                for k, (start_index, end_index) in enumerate(zip(bout_start_indices, bout_end_indices)):
                    if np.sum(trial_frequency[start_index:end_index]) == 0:
                        if use_amplitude_exclusion:
                            if np.max(trial_amplitude[start_index:end_index]) == np.min(trial_amplitude[start_index:end_index]):
                                false_bouts.append(k)
                        else:
                            false_bouts.append(k)
                if len(false_bouts) > 0:
                    false_bouts = np.array(false_bouts)
                    bout_start_indices = np.delete(bout_start_indices, false_bouts)
                    bout_end_indices = np.delete(bout_end_indices, false_bouts)
                all_bout_start_indices[fish_index][trial_index] = bout_start_indices
                all_bout_end_indices[fish_index][trial_index] = bout_end_indices
                n_bouts_detected = bout_start_indices.shape[0]
                sum_instances[fish_index][trial_index] = n_bouts_detected
                if n_bouts_detected > 0:
                    bout_detected_in_trial[fish_index][trial_index] = 1
    return [np.array(all_bout_start_indices), np.array(all_bout_end_indices), sum_instances, bout_detected_in_trial]


def calculate_bout_durations_interbout_durations_latency_mean_tail_beat_frequency(sum_instances, frequency, bout_start_indices, bout_end_indices):
    bout_durations = [[np.array([]) for j in i] for i in np.zeros(sum_instances.shape)]
    interbout_durations = [[np.array([]) for j in i] for i in np.zeros(sum_instances.shape)]
    latencies = [[np.array([]) for j in i] for i in np.zeros(sum_instances.shape)]
    mean_tail_beat_frequencies =  [[np.array([]) for j in i] for i in np.zeros(sum_instances.shape)]
    for fish_index, fish_sum_instance in enumerate(sum_instances):
        for index, sum_instance in enumerate(fish_sum_instance):
            if sum_instance > 0:
                bout_durations[fish_index][index] = (bout_end_indices[fish_index][index] - bout_start_indices[fish_index][index])
                latencies[fish_index][index] = np.array([bout_start_indices[fish_index][index][0]])
                mean_tail_beat_frequencies[fish_index][index] = np.array([np.mean(frequency[fish_index][index][bout_start_index:bout_end_index]) for bout_start_index, bout_end_index in zip(bout_start_indices[fish_index][index], bout_end_indices[fish_index][index])])
            if sum_instance > 1:
                interbout_durations[fish_index][index] = bout_start_indices[fish_index][index][1:] - bout_end_indices[fish_index][index][:-1]
    return [np.array(bout_durations), np.array(interbout_durations), np.array(latencies), np.array(mean_tail_beat_frequencies)]

def sort_arr_by_gain(arr, gain_values):
    return np.array([np.array([np.array([trial_arr for index, trial_arr in enumerate(fish_arr) if gain_value[index] == gain_value_index]) for fish_arr, gain_value in zip(arr, gain_values)]) for gain_value_index in np.arange(0, 2, 0.5)])

def get_index_of_file_basename(file_basename, phase_csvs):
    return [i for i in range(len(phase_csvs)) if file_basename in phase_csvs[i]][0]

def generate_highlighted_regions(phase, trial_indices, gain_values):
    regions_to_highlight = np.ones(phase.shape[0]) * -1
    for trial_index, gain_value in zip(trial_indices, gain_values):
        regions_to_highlight[trial_index[0]:trial_index[1]] = gain_value
    return regions_to_highlight

def calculate_variables(data, imaging_FPS = 332):
    # Create useful variable names
    phase_csvs, phase, trial_num, trial_indices, tail_curvature, frequency, amplitude, instance, gain_values = data
    # Process data and calculate new variables
    sorted_tail_curvature = sort_arr_by_gain(tail_curvature, gain_values)
    sorted_frequency = sort_arr_by_gain(frequency, gain_values)
    sorted_amplitude = sort_arr_by_gain(amplitude, gain_values)
    sorted_instance = sort_arr_by_gain(instance, gain_values)
    bout_indices_sum_instances_bout_detected = np.array([calculate_bout_indices_sum_instances_bout_detected(gain_instance, gain_frequency, gain_amplitude) for gain_instance, gain_frequency, gain_amplitude in zip(sorted_instance, sorted_frequency, sorted_amplitude)])
    bout_durations_interbout_durations_latency_mean_tail_beat_frequency = np.array([calculate_bout_durations_interbout_durations_latency_mean_tail_beat_frequency(gain_bout_indices_sum_instances_bout_detected[2], gain_frequency, gain_bout_indices_sum_instances_bout_detected[0], gain_bout_indices_sum_instances_bout_detected[1]) for gain_frequency, gain_bout_indices_sum_instances_bout_detected in zip(sorted_frequency, bout_indices_sum_instances_bout_detected)])
    bout_start_indices, bout_end_indices, sum_instances, bout_detected = np.swapaxes(bout_indices_sum_instances_bout_detected, 0, 1)
    bout_durations, interbout_durations, latency, mean_tail_beat_frequency = np.swapaxes(bout_durations_interbout_durations_latency_mean_tail_beat_frequency, 0, 1)
    bout_durations = bout_durations / imaging_FPS * 1000
    interbout_durations = interbout_durations / imaging_FPS
    latency = latency / imaging_FPS
    return [bout_start_indices, bout_end_indices, sum_instances, bout_detected, bout_durations, interbout_durations, latency, mean_tail_beat_frequency]

def calculate_means_and_sems(variables):
    # Create useful variable names
    bout_start_indices, bout_end_indices, sum_instances, bout_detected, bout_durations, interbout_durations, latency, max_tail_beat_frequency = variables
    # Calculate means and sems
    mean_n_bouts = np.mean(np.mean(sum_instances, axis = 2), axis = 1)
    sem_n_bouts = np.array([stats.sem(gain_sum_instances) for gain_sum_instances in np.mean(sum_instances, axis = 2)])
    mean_bout_durations = np.mean(np.array([[np.mean(np.concatenate(fish_bout_durations.ravel())) for fish_bout_durations in gain_bout_durations] for gain_bout_durations in bout_durations]), axis = 1)
    sem_bout_durations = np.array([stats.sem([np.mean(np.concatenate(fish_bout_durations.ravel())) for fish_bout_durations in gain_bout_durations]) for gain_bout_durations in bout_durations])
    mean_interbout_durations = np.mean(np.array([[np.mean(np.concatenate(fish_interbout_durations.ravel())) for fish_interbout_durations in gain_interbout_durations] for gain_interbout_durations in interbout_durations]), axis = 1)
    sem_interbout_durations = np.array([stats.sem([np.mean(np.concatenate(fish_interbout_durations.ravel())) for fish_interbout_durations in gain_interbout_durations]) for gain_interbout_durations in interbout_durations])
    mean_latency = np.mean(np.array([[np.mean(np.concatenate(fish_latency.ravel())) for fish_latency in gain_latency] for gain_latency in latency]), axis = 1)
    sem_latency = np.array([stats.sem([np.mean(np.concatenate(fish_latency.ravel())) for fish_latency in gain_latency]) for gain_latency in latency])
    mean_max_tail_beat_frequency = np.mean(np.array([[np.mean(np.concatenate(fish_max_tail_beat_frequency.ravel())) for fish_max_tail_beat_frequency in gain_max_tail_beat_frequency] for gain_max_tail_beat_frequency in max_tail_beat_frequency]), axis = 1)
    sem_max_tail_beat_frequency = np.array([stats.sem([np.mean(np.concatenate(fish_max_tail_beat_frequency.ravel())) for fish_max_tail_beat_frequency in gain_max_tail_beat_frequency]) for gain_max_tail_beat_frequency in max_tail_beat_frequency])
    return [mean_n_bouts, sem_n_bouts, mean_bout_durations, sem_bout_durations, mean_interbout_durations, sem_interbout_durations, mean_latency, sem_latency, mean_max_tail_beat_frequency, sem_max_tail_beat_frequency]

def plot_results(results, save_path = None):
    mean_n_bouts, sem_n_bouts, mean_bout_durations, sem_bout_durations, mean_interbout_durations, sem_interbout_durations, mean_latency, sem_latency, mean_max_tail_beat_frequency, sem_max_tail_beat_frequency = results
    fig = plt.figure(figsize = (20, 4))
    gs = fig.add_gridspec(ncols = 5, nrows = 1, wspace = 0.2)
    n_bouts_ax = fig.add_subplot(gs[0, 0])
    bout_duration_ax = fig.add_subplot(gs[0, 1])
    interbout_duration_ax = fig.add_subplot(gs[0, 2])
    latency_ax = fig.add_subplot(gs[0, 3])
    max_tbf_ax = fig.add_subplot(gs[0, 4])
    n_bouts_ax.errorbar(range(len(mean_n_bouts)), mean_n_bouts, sem_n_bouts, color = "blue", capsize = 10, elinewidth = 4, capthick = 1, ecolor = "black", linewidth = 6, markerfacecolor = "white", marker = "o", markersize = 20, markeredgewidth = 4)
    bout_duration_ax.errorbar(range(len(mean_bout_durations)), mean_bout_durations, sem_bout_durations, color = "blue", capsize = 10, elinewidth = 4, capthick = 1, ecolor = "black", linewidth = 6, markerfacecolor = "white", marker = "o", markersize = 20, markeredgewidth = 4)
    interbout_duration_ax.errorbar(range(len(mean_interbout_durations)), mean_interbout_durations, sem_interbout_durations, color = "blue", capsize = 10, elinewidth = 4, capthick = 1, ecolor = "black", linewidth = 6, markerfacecolor = "white", marker = "o", markersize = 20, markeredgewidth = 4)
    latency_ax.errorbar(range(len(mean_latency)), mean_latency, sem_latency, color = "blue", capsize = 10, elinewidth = 4, capthick = 1, ecolor = "black", linewidth = 6, markerfacecolor = "white", marker = "o", markersize = 20, markeredgewidth = 4)
    max_tbf_ax.errorbar(range(len(mean_max_tail_beat_frequency)), mean_max_tail_beat_frequency, sem_max_tail_beat_frequency, color = "blue", capsize = 10, elinewidth = 4, capthick = 1, ecolor = "black", linewidth = 6, markerfacecolor = "white", marker = "o", markersize = 20, markeredgewidth = 4)
    # Save the plot and show
    if save_path is not None:
        plt.savefig(save_path)
    plt.show()

def calculate_ANOVA(variables):
    bout_start_indices, bout_end_indices, sum_instances, bout_detected, bout_durations, interbout_durations, latency, max_tbf = variables
    print("ANOVA RESULTS\n=============\n")
    ANOVA_sum_instances = np.mean(sum_instances, axis = 2)
    print("N bouts: {}".format(stats.f_oneway(ANOVA_sum_instances[0], ANOVA_sum_instances[1], ANOVA_sum_instances[2], ANOVA_sum_instances[3])))
    ANOVA_bout_durations = np.array([[np.mean(np.concatenate(fish_bout_durations.ravel())) for fish_bout_durations in gain_bout_durations] for gain_bout_durations in bout_durations])
    print("Bout durations: {}".format(stats.f_oneway(ANOVA_bout_durations[0], ANOVA_bout_durations[1], ANOVA_bout_durations[2], ANOVA_bout_durations[3])))
    ANOVA_interbout_durations = np.array([[np.mean(np.concatenate(fish_interbout_durations.ravel())) for fish_interbout_durations in gain_interbout_durations] for gain_interbout_durations in interbout_durations])
    print("Interbout durations: {}".format(stats.f_oneway(ANOVA_interbout_durations[0], ANOVA_interbout_durations[1], ANOVA_interbout_durations[2], ANOVA_interbout_durations[3])))
    ANOVA_latency = np.array([[np.mean(np.concatenate(fish_latency.ravel())) for fish_latency in gain_latency] for gain_latency in latency])
    print("Latency: {}".format(stats.f_oneway(ANOVA_latency[0], ANOVA_latency[1], ANOVA_latency[2], ANOVA_latency[3])))
    ANOVA_tbf = np.array([[np.mean(np.concatenate(fish_tbf.ravel())) for fish_tbf in gain_tbf] for gain_tbf in max_tbf])
    print("Max Tail Beat Frequency: {}".format(stats.f_oneway(ANOVA_tbf[0], ANOVA_tbf[1], ANOVA_tbf[2], ANOVA_tbf[3])))

def calculate_pairwise_tukeyhsd(variables):
    bout_start_indices, bout_end_indices, sum_instances, bout_detected, bout_durations, interbout_durations, latency, max_tbf = variables
    print("Tukey Post-Hoc Comparisons\n======================\n")
    group_labels = np.repeat(np.array(["Gain 0.0", "Gain 0.5", "Gain 1.0", "Gain 1.5"]), sum_instances.shape[1]).ravel()
    tukeyhsd_sum_instances = np.mean(sum_instances, axis = 2).ravel().astype(float)
    print("N Bouts: {}".format(pairwise_tukeyhsd(tukeyhsd_sum_instances, group_labels).summary()))
    tukeyhsd_bout_durations = np.array([[np.mean(np.concatenate(fish_bout_durations.ravel())) for fish_bout_durations in gain_bout_durations] for gain_bout_durations in bout_durations]).ravel()
    print("Bout Durations: {}".format(pairwise_tukeyhsd(tukeyhsd_bout_durations, group_labels).summary()))
    tukeyhsd_interbout_durations = np.array([[np.mean(np.concatenate(fish_interbout_durations.ravel())) for fish_interbout_durations in gain_interbout_durations] for gain_interbout_durations in interbout_durations]).ravel()
    print("Interbout Durations: {}".format(pairwise_tukeyhsd(tukeyhsd_interbout_durations, group_labels).summary()))
    tukeyhsd_latency = np.array([[np.mean(np.concatenate(fish_latency.ravel())) for fish_latency in gain_latency] for gain_latency in latency]).ravel()
    print("Latency: {}".format(pairwise_tukeyhsd(tukeyhsd_latency, group_labels).summary()))
    tukeyhsd_tbf = np.array([[np.mean(np.concatenate(fish_tbf.ravel())) for fish_tbf in gain_tbf] for gain_tbf in max_tbf]).ravel()
    print("Max Tail Beat Frequency: {}".format(pairwise_tukeyhsd(tukeyhsd_tbf, group_labels).summary()))

def calculate_1d_closed_loop_data(ordered_data, ordered_timestamps, gain_value, exclude_short_bouts = False, short_bout_threshold = None):
    trial_starts = np.where(np.diff(np.logical_and(ordered_data[0] == 1, ordered_data[1] == gain_value).astype(int)) == 1)[0]
    trial_ends = np.where(np.diff(np.logical_and(ordered_data[0] == 1, ordered_data[1] == gain_value).astype(int)) == -1)[0]
    time_starts = [ordered_timestamps[start] for start in trial_starts]
    time_ends = [ordered_timestamps[end] for end in trial_ends]
    instances = [ordered_data[6][start:end] for i, (start, end) in enumerate(zip(trial_starts, trial_ends)) if time_ends[i]-time_starts[i]>29]
    frequency = [ordered_data[5][start:end] for i, (start, end) in enumerate(zip(trial_starts, trial_ends)) if time_ends[i]-time_starts[i]>29]
    bout_starts = [np.where(np.diff(inst) == 1)[0] if len(np.where(np.diff(inst) == 1)[0]) > 0 else np.nan for inst in instances]
    bout_ends = [np.where(np.diff(inst) == -1)[0] if len(np.where(np.diff(inst) == -1)[0]) > 0 else np.nan for inst in instances]
    for i, (starts, ends) in enumerate(zip(bout_starts, bout_ends)):
        if len(starts) > 0 and len(ends) > 0:
            if ends[0] < starts[0]:
                bout_ends[i] = np.delete(bout_ends[i], 0)
            if starts[-1] > ends[-1]:
                bout_starts[i] = np.delete(bout_starts[i], -1)
    if exclude_short_bouts and short_bout_threshold is not None:
        for i, (starts, ends) in enumerate(zip(bout_starts, bout_ends)):
            exclusion_indices = []
            for j, (start, end) in enumerate(zip(starts, ends)):
                if end-start < short_bout_threshold:
                    exclusion_indices.append(j)
            bout_starts[i] = np.delete(bout_starts[i], exclusion_indices)
            bout_ends[i] = np.delete(bout_ends[i], exclusion_indices)
    n_trials = len(trial_starts)
    n_bouts = [len(start) if not np.isnan(start) else np.nan for start in bout_starts]
    responsive_trials = np.sum([1 for val in n_bouts if val != 0 or not np.isnan(val)])
    durations = [(end-start)*1000/332 for start, end in zip(bout_starts, bout_ends)]
    interbout_durations = [(start[1:]-end[:-1])*1000/332 for start, end in zip(bout_starts, bout_ends)]
    mean_tbf = [[np.mean(frequency[i][bout_start:bout_end]) for bout_start, bout_end in zip(trial_starts, trial_ends)] for i, (trial_starts, trial_ends) in enumerate(zip(bout_starts, bout_ends))]
    n_bouts = np.mean(n_bouts)
    durations = np.mean([np.mean(val) for val in durations if len(val) > 0]) if len([np.mean(val) for val in durations if len(val) > 0]) > 0 else np.nan
    interbout_durations = np.mean([np.mean(val) for val in interbout_durations if len(val) > 0]) if len([np.mean(val) for val in interbout_durations if len(val) > 0]) > 0 else np.nan
    mean_tbf = np.mean([np.mean(val) for val in mean_tbf if len(val) > 0]) if len([np.mean(val) for val in mean_tbf if len(val) > 0]) else np.nan
    return [n_bouts, durations, interbout_durations, mean_tbf, n_trials, responsive_trials]

def remove_NaNs_from_data(data):
    return [val for val in data if not np.isnan(val)]
