-- Fix invoices where balance is 0 but status is not Paid/Cancelled/Draft
UPDATE Invoice
SET    Status = 'Paid'
WHERE  (TotalAmount - ReceivedAmount) <= 0
  AND  Status NOT IN ('Paid', 'Cancelled', 'Draft');
