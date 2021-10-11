#sudo docker pull mcr.microsoft.com/mssql/server:2019-latest
docker stop sql1 && docker rm sql1

docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Aa345678" \
   -p 1433:1433 --name sql1 -h sql1 \
   -d mcr.microsoft.com/mssql/server:2019-latest

docker cp Historical_Data/ sql1:/
docker cp connect.sh sql1:/
docker cp data_insert.sql sql1:/
