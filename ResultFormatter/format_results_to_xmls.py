import xlsxwriter
import os
import sys
import json
from os import walk
from os.path import join
from tqdm import tqdm

class bcolors:
    DEFAULT = "\033[39m"
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKCYAN = '\033[96m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'

GOAL_FUNCTIONS = {
    "1": [35, 60, 5],
    "2": [15, 80, 5],
    "3": [45, 45, 10],
    "4": [20, 80, 0],
    "5": [20, 70, 10],
    "6": [20, 60, 20],
    "7": [20, 50, 30],
    "8": [20, 40, 40],
    "9": [20, 30, 50],
    "10": [20, 20, 60],
    "11": [20, 10, 70],
    "12": [20, 0, 80],
}

def get_results_files(location):
    for (dirpath, dirnames, filenames) in walk(location):
        return [join(location, file) for file in filenames]

def decode_json(filename):
    with open(filename, 'r', encoding='utf-8-sig') as file:
        return json.load(file)

def process_file(filename, worksheet, workbook, start_row, goal_fun_factors, print_goal_factors):
    results_per_file = decode_json(filename)

    col = 1

    if print_goal_factors:
        goal_function_str = f"Goal function: U - {goal_fun_factors[0]}%, P - {goal_fun_factors[1]}%, T - {goal_fun_factors[1]}%"
        worksheet.merge_range(f'B{start_row+1}:D{start_row+1}', goal_function_str, add_bg_format_with_border(workbook, 'green'))

        start_row += 2

        headers = ['Multi criteria', 'Tree search', 'Amount of jobs', 'Goal function value', 'Utility',
            'Profit', 'Time', 'Duration of scheduling [ms]', 'Duration of multi criteria [ms]', 'Nodes', 'Breaks']

        for title in headers:
            worksheet.write(start_row, col, title, add_bg_format_with_border(workbook, 'yellow'))
            col += 1

        worksheet.merge_range(f'M{start_row+1}:W{start_row+1}', 'Solution', add_bg_format_with_border(workbook, 'yellow'))

    processing_row = start_row + 1
    
    for result in results_per_file:
        save_results_to_worksheet(result, worksheet, workbook, processing_row)
        processing_row += 6

    return processing_row

def save_results_to_worksheet(result, worksheet, workbook, start_row):
    col = 1

    # making row of values
    alg = result['AlgorithmName'].split('\u002B')
    tree_search = alg[0]
    multi_criteria = alg[1]
    row_values = [multi_criteria, tree_search, result['AmountOfJobs'], result['Value'], result['Factors'][0],
        result['Factors'][1], result['Factors'][2], result['Duration'], result['CriteriaDuration'], result['VisitedNodes'],
        len(result['Breaks'])]

    for value in row_values:
        worksheet.write(start_row, col, value, add_border_format(workbook))
        col += 1

    worksheet.merge_range(f'M{start_row+1}:W{start_row+6}', '\n'.join([get_solution_str(sol) for sol in result['Jobs']]),
        add_border_format_with_wrap(workbook))

def get_solution_str(solution):
    from_name = solution['From']['Name']
    to_name = solution['To']['Name']
    price = solution['Price']
    pickup = f"({solution['Pickup']['Item1']}, {solution['Pickup']['Item2']})"
    delivery = f"({solution['Delivery']['Item1']}, {solution['Delivery']['Item2']})"
    loading = solution['LoadingTime']
    return f"{from_name} -> {to_name}, Price: {price}, Pickup: {pickup}, Delivery: {delivery}, Loading: {loading}"

def process_results(dirname, results_path, workbook):
    print(f'{bcolors.OKCYAN}🧐 Processing: {dirname}')
    print(f'{bcolors.WARNING}')

    worksheet = workbook.add_worksheet(dirname)
    results_files = get_results_files(results_path)
    sorted_results = list(filter(lambda file: file.find("GF1") >= 0, results_files))
    sorted_results.extend(list(filter(lambda file: file.find("GF2") >= 0, results_files)))
    sorted_results.extend(list(filter(lambda file: file.find("GF3") >= 0, results_files)))

    processing_row = 0
    goal_fun_factors = []

    for file in tqdm(sorted_results):
        splitted = file.split(".json")[0]
        number = splitted[len(splitted) - 1]
        new_goal_fun_factors = GOAL_FUNCTIONS[number]
        processing_row = process_file(file, worksheet, workbook, processing_row, new_goal_fun_factors,
            goal_fun_factors != new_goal_fun_factors)
        goal_fun_factors = new_goal_fun_factors

def add_bg_format_with_border(workbook, bg_color):
    format = workbook.add_format({'bg_color': bg_color})
    format.set_border()
    return format

def add_border_format(workbook):
    format = workbook.add_format()
    format.set_border()
    return format

def add_border_format_with_wrap(workbook):
    format = workbook.add_format()
    format.set_border()
    format.set_text_wrap()
    format.set_align('vcenter')
    return format

def main():
    if (len(sys.argv) < 2):
        print(f'{bcolors.FAIL}❌ Not enought params in input')
        print(bcolors.DEFAULT)
        return;

    results_location = sys.argv[1]

    all_paths = []
    # Create a workbook and add a worksheet.
    workbook = xlsxwriter.Workbook('ResultsAll.xlsx')

    for (dirpath, dirnames, filenames) in walk(results_location):
        all_paths = [(dirname, join(dirpath, dirname)) for dirname in dirnames]
        break;

    for dirname, path in all_paths:
        process_results(dirname, path, workbook)

    print(f'{bcolors.OKGREEN}💪 Processing completed')
    print(bcolors.DEFAULT)

    workbook.close()

if __name__ == "__main__":
    main()