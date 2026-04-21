using FluentValidation;
using learnnet.DTOs;

namespace learnnet.Validators
{
    /* 
     * INPUT VALIDATION (Kiểm tra dữ liệu đầu vào):
     * Chúng ta sử dụng FluentValidation thay vì DataAnnotations truyền thống
     * để giữ cho các DTOs (Data Transfer Objects) được sạch sẽ và dễ bảo trì.
     * Đây là cách tiếp cận chuyên nghiệp (Production-ready) trong các hệ thống lớn.
     */
    public class TaskCreateDtoValidator : AbstractValidator<TaskCreateDto>
    {
        public TaskCreateDtoValidator()
        {
            // Kiểm tra Tiêu đề không được trống và có độ dài hợp lý
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tiêu đề không được để trống.")
                .MaximumLength(200).WithMessage("Tiêu đề không được quá 200 ký tự.");

            // Kiểm tra Mô tả có độ dài tối đa
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Mô tả không được quá 1000 ký tự.");

            // Kiểm tra Thời hạn hoàn thành: Không được phép đặt deadline ở quá khứ
            RuleFor(x => x.Deadline)
                .Must(deadline => !deadline.HasValue || deadline.Value > DateTime.UtcNow)
                .WithMessage("Thời hạn hoàn thành không được ở trong quá khứ.");
        }
    }

    public class TaskUpdateDtoValidator : AbstractValidator<TaskUpdateDto>
    {
        public TaskUpdateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tiêu đề không được để trống.")
                .MaximumLength(200).WithMessage("Tiêu đề không được quá 200 ký tự.");

            /* 
             * BẮT BUỘC: Kiểm tra ConcurrencyToken.
             * Đây là "chìa khóa" để cơ chế Optimistic Locking hoạt động.
             * Client phải gửi lại Token mà họ đã nhận được khi đọc dữ liệu.
             */
            RuleFor(x => x.ConcurrencyToken)
                .NotEmpty().WithMessage("Thiếu ConcurrencyToken để kiểm tra tranh chấp dữ liệu.");
        }
    }
}
