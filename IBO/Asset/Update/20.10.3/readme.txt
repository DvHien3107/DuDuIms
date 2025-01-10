1. update lại trang đăng ký mới
    + Update lại logic form và giao diện ở trang đăng ký mới
    + update value L_Type cho biết dữ liệu sale lead có từ đâu
2. update lại trang sale lead 
    + Thêm Type cho table Calendar_Event(event,log) để phân biệt giữa log và event
    + Thêm SalesLeadId và SalesLeadName cho table Calendar_Event , thay đổi lấy lịch sử appointment  dựa vào customer code => sang SaleLead Id
    + Update lại quan hệ giữa 2 table Customer và SaleLead từ Customer_Id và SaleLead_Id  sang customer_code
4. update lại trang merchant dashboard
    + thêm tính năng lọc theo thời hạn còn lại (>=30 ngày,>=15ngày,=<15ngày, hết hạn)
    + remove required salon phone của form new merchant
3. Update lại cơ chế tạo verify trial
    + rollback lại toàn bộ giữ liệu nếu khách hàng xác nhận bị lỗi
    + tách hàm send api create store tạo trial ra riêng biệt (trước đó là aprove action) để tránh những hàm thừa -> tăng tốc độ