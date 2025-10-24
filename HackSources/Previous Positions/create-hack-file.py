import os
import subprocess
import glob
from pathlib import Path

def combine_bin_files(input_dir, output_file):
    with open(output_file, "w") as out:
        for filename in sorted(os.listdir(input_dir)):
            if filename.lower().endswith(".bin"):
                file_path = os.path.join(input_dir, filename)
                with open(file_path, "rb") as f:
                    data = f.read()
                    hex_str = " ".join(f"{byte:02X}" for byte in data)
                    base_name = os.path.splitext(filename)[0]
                    out.write(f"{base_name}: {hex_str}\n")

if __name__ == f"__main__":
    for rom in ["US", "J"]:
        input_directory = f"./build/{rom}"

        # clear potentially unwanted lingering bin files
        bin_files = glob.glob(f'{input_directory}/*.bin')
        for f in bin_files:
            os.remove(f)

        # assemble the code using armips
        Path(input_directory).mkdir(parents=True, exist_ok=True)
        subprocess.run(["armips", f"Previous Positions ({rom}).asm"])

        # combine the output into the .hck file
        output_filename = f"./build/{rom}/Previous Positions ({rom}).hck"
        combine_bin_files(input_directory, output_filename)
        print(f"Combined binary data written to {output_filename}")
