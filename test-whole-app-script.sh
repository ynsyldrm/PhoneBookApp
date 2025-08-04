#!/bin/bash
# Tam test senaryosu script

echo "=== Phone Book API Test Başlıyor ==="

# 1. Kişi oluştur
echo "1. Kişi oluşturuluyor..."
CONTACT1=$(curl -s -X POST http://localhost:5000/api/contacts \
  -H "Content-Type: application/json" \
  -d '{"firstName": "Ahmet", "lastName": "Yılmaz", "company": "Test A.Ş."}' | jq -r '.id')

echo "Kişi ID: $CONTACT1"

# 2. İletişim bilgisi ekle
echo "2. İletişim bilgisi ekleniyor..."
curl -s -X POST http://localhost:5000/api/contacts/$CONTACT1/contact-info \
  -H "Content-Type: application/json" \
  -d '{"infoType": 0, "infoContent": "+90 532 123 45 67"}'

curl -s -X POST http://localhost:5000/api/contacts/$CONTACT1/contact-info \
  -H "Content-Type: application/json" \
  -d '{"infoType": 2, "infoContent": "İstanbul"}'

# 3. Rapor talep et
echo "3. Rapor talep ediliyor..."
REPORT_ID=$(curl -s -X POST http://localhost:5000/api/reports/request | jq -r '.reportId')
echo "Rapor ID: $REPORT_ID"

# 4. Rapor durumunu kontrol et
echo "4. Rapor işlenmesi bekleniyor..."
sleep 5

echo "5. Rapor sonucu:"
curl -s http://localhost:5000/api/reports/$REPORT_ID | jq

echo "=== Test Tamamlandı ==="