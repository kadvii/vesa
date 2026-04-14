namespace eVisaPlatform.Domain.Enums;

public enum VisaStatus
{
    Pending        = 1,
    UnderReview    = 2,
    Approved       = 3,
    Rejected       = 4,
    PendingPayment = 5,   // Approved but fee not yet collected
    Paid           = 6,   // Fee confirmed by payment gateway callback
}
