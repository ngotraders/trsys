if [ ! -e "/etc/nginx/ssl-cert/server.crt" ]; then
    mkdir /etc/nginx/ssl-cert
    openssl req -x509 -newkey rsa:4096 -nodes -subj '/CN=localhost' -keyout /etc/nginx/ssl-cert/server.key -out /etc/nginx/ssl-cert/server.crt -days 365
fi