FROM python:3.11-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY ./ .
RUN chmod -R -755 server/

EXPOSE 8080

CMD ["waitress-serve", "--port=8080", "server:app"]
