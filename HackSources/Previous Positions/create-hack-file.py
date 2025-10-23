import os

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

if __name__ == "__main__":
    input_directory = "./build/US"
    output_filename = "./build/US/Previous Positions (US).hck"
    combine_bin_files(input_directory, output_filename)
    print(f"Combined binary data written to {output_filename}")
