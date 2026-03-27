#!/bin/zsh
dotnet ef migrations add migEmpCode
 dotnet ef database update
set -e  # Dừng nếu có lỗi

echo "🛠️ Building project..."
rm -rf ./publish && dotnet publish ./yMoi.csproj -c Release -o publish && cd publish && zip -r ../app.zip . > /dev/null && cd ..

# Đảm bảo file app.zip tồn tại và không bị ghi nữa
# echo "⏳ Waiting for zip to finish..."
# sleep 1
# sync  # Đồng bộ ghi file (bắt OS flush file IO buffer)

# if [[ ! -f app.zip ]]; then
#   echo "❌ app.zip not found."
#   exit 1
# fi

# echo "🚀 Uploading..."
# lftp -u site30668,C-j48zL+qQ=6 site30668.siteasp.net <<EOF
# set ssl:verify-certificate no
# set net:max-retries 2
# set net:timeout 20
# set net:idle 5
# set ftp:sync-mode off
# set cmd:fail-exit yes

# cd wwwroot
# put -O . app.zip
# bye
# EOF

# echo "✅ Upload completed!"
