namespace eVisaPlatform.Application.Email;

/// <summary>
/// Localised HTML email templates for visa application lifecycle events.
/// All templates are Arabic-first with embedded RTL styling.
/// </summary>
public static class VisaEmailTemplates
{
    // ── Shared layout ─────────────────────────────────────────────────────────
    private static string Wrap(string accentColor, string icon, string heading, string body, string footer) => $"""
        <!DOCTYPE html>
        <html lang="ar" dir="rtl">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>Visa Z — إشعار</title>
        </head>
        <body style="margin:0;padding:0;background:#0a0e1a;font-family:'Segoe UI',Tahoma,Arial,sans-serif;direction:rtl;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#0a0e1a;padding:40px 20px;">
            <tr>
              <td align="center">
                <table width="600" cellpadding="0" cellspacing="0"
                       style="background:linear-gradient(145deg,#1a1f35,#0f1220);
                              border:1px solid rgba(255,255,255,0.08);
                              border-radius:20px;overflow:hidden;">

                  <!-- Header bar -->
                  <tr>
                    <td style="background:{accentColor};padding:6px 0;"></td>
                  </tr>

                  <!-- Logo row -->
                  <tr>
                    <td align="center" style="padding:32px 40px 0;">
                      <div style="font-size:36px;">{icon}</div>
                      <div style="margin-top:10px;">
                        <span style="font-size:22px;font-weight:800;color:#d4af37;letter-spacing:-0.5px;">Visa Z</span>
                        <span style="font-size:12px;color:#8899aa;margin-right:6px;">بوابة التأشيرات الإلكترونية</span>
                      </div>
                    </td>
                  </tr>

                  <!-- Heading -->
                  <tr>
                    <td align="center" style="padding:24px 40px 0;">
                      <h1 style="margin:0;font-size:22px;font-weight:800;color:#e8dcc8;">{heading}</h1>
                    </td>
                  </tr>

                  <!-- Body card -->
                  <tr>
                    <td style="padding:24px 40px;">
                      <div style="background:rgba(255,255,255,0.04);border:1px solid rgba(255,255,255,0.08);
                                  border-radius:14px;padding:24px;">
                        {body}
                      </div>
                    </td>
                  </tr>

                  <!-- Footer -->
                  <tr>
                    <td align="center" style="padding:16px 40px 32px;">
                      <p style="font-size:11px;color:#555e6e;margin:0;">
                        {footer}
                      </p>
                      <p style="font-size:11px;color:#3a4255;margin:6px 0 0;">
                        هذا البريد أُرسل تلقائياً — يرجى عدم الرد عليه.
                      </p>
                    </td>
                  </tr>

                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;

    // ── APPROVED ──────────────────────────────────────────────────────────────
    /// <summary>
    /// Render the "Visa Approved" HTML email.
    /// </summary>
    /// <param name="applicantName">Full name of the applicant.</param>
    /// <param name="visaType">e.g. "Tourist", "Business".</param>
    /// <param name="destination">Destination country.</param>
    /// <param name="applicationId">GUID of the visa application for reference.</param>
    /// <param name="reviewerName">Name/email of the approving officer.</param>
    /// <param name="reviewDate">Date of approval.</param>
    public static string Approved(
        string applicantName,
        string visaType,
        string destination,
        Guid   applicationId,
        string reviewerName,
        DateTime reviewDate)
    {
        var refCode = $"VZ-{applicationId:N}".ToUpper()[..12];
        var dateStr = reviewDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("ar-SA"));

        var body = $"""
            <p style="font-size:15px;color:#c8b89a;margin:0 0 16px;">
              مرحباً، <strong style="color:#e8dcc8;">{System.Net.WebUtility.HtmlEncode(applicantName)}</strong>
            </p>
            <p style="font-size:14px;color:#8899aa;margin:0 0 20px;line-height:1.8;">
              يسعدنا إبلاغك بأن طلب التأشيرة الخاص بك قد <strong style="color:#4ade80;">تمت الموافقة عليه</strong>. 
              يمكنك مراجعة تفاصيل الطلب من خلال بوابة Visa Z.
            </p>

            <!-- Details table -->
            <table width="100%" cellpadding="0" cellspacing="0">
              {Row("نوع التأشيرة",    visaType)}
              {Row("دولة الوجهة",     destination)}
              {Row("رمز المرجع",      refCode)}
              {Row("تاريخ الموافقة",  dateStr)}
              {Row("تمت المراجعة بواسطة", reviewerName)}
            </table>

            <!-- Next steps -->
            <div style="margin-top:20px;background:rgba(74,222,128,0.08);
                        border:1px solid rgba(74,222,128,0.22);border-radius:10px;padding:14px 18px;">
              <p style="margin:0;font-size:13px;color:#4ade80;font-weight:700;">الخطوة التالية</p>
              <p style="margin:6px 0 0;font-size:13px;color:#8899aa;line-height:1.7;">
                يُرجى إتمام سداد رسوم التأشيرة عبر بوابة Visa Z للمضي قدماً في معالجة طلبك.
              </p>
            </div>
            """;

        return Wrap(
            accentColor: "#4ade80",
            icon:        "✅",
            heading:     "تهانينا! تمت الموافقة على تأشيرتك",
            body:        body,
            footer:      $"© {DateTime.UtcNow.Year} Visa Z · جميع الحقوق محفوظة"
        );
    }

    // ── REJECTED ──────────────────────────────────────────────────────────────
    /// <summary>
    /// Render the "Visa Rejected" HTML email.
    /// </summary>
    public static string Rejected(
        string applicantName,
        string visaType,
        string destination,
        Guid   applicationId,
        string reason,
        DateTime reviewDate)
    {
        var refCode = $"VZ-{applicationId:N}".ToUpper()[..12];
        var dateStr = reviewDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("ar-SA"));
        var safeReason = string.IsNullOrWhiteSpace(reason) ? "لم يُحدد سبب." : System.Net.WebUtility.HtmlEncode(reason);

        var body = $"""
            <p style="font-size:15px;color:#c8b89a;margin:0 0 16px;">
              عزيزي/عزيزتي، <strong style="color:#e8dcc8;">{System.Net.WebUtility.HtmlEncode(applicantName)}</strong>
            </p>
            <p style="font-size:14px;color:#8899aa;margin:0 0 20px;line-height:1.8;">
              نأسف لإعلامك بأن طلب التأشيرة الخاص بك قد <strong style="color:#f87171;">تم رفضه</strong>.
              يمكنك التقدم بطلب جديد بعد تصحيح الأسباب المذكورة أدناه.
            </p>

            <table width="100%" cellpadding="0" cellspacing="0">
              {Row("نوع التأشيرة",   visaType)}
              {Row("دولة الوجهة",    destination)}
              {Row("رمز المرجع",     refCode)}
              {Row("تاريخ القرار",   dateStr)}
            </table>

            <div style="margin-top:20px;background:rgba(248,113,113,0.08);
                        border:1px solid rgba(248,113,113,0.22);border-radius:10px;padding:14px 18px;">
              <p style="margin:0;font-size:13px;color:#f87171;font-weight:700;">سبب الرفض</p>
              <p style="margin:6px 0 0;font-size:13px;color:#8899aa;line-height:1.7;">{safeReason}</p>
            </div>
            """;

        return Wrap(
            accentColor: "#f87171",
            icon:        "❌",
            heading:     "إشعار برفض طلب التأشيرة",
            body:        body,
            footer:      $"© {DateTime.UtcNow.Year} Visa Z · للاستفسار تواصل مع الدعم"
        );
    }

    // ── SUBMISSION CONFIRMATION ───────────────────────────────────────────────
    /// <summary>
    /// Render the "Application Received" confirmation email sent immediately on submission.
    /// </summary>
    public static string Submitted(
        string applicantName,
        string visaType,
        string destination,
        Guid   applicationId,
        DateTime submissionDate)
    {
        var refCode = $"VZ-{applicationId:N}".ToUpper()[..12];
        var dateStr = submissionDate.ToString("dd MMMM yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture) + " UTC";

        var body = $"""
            <p style="font-size:15px;color:#c8b89a;margin:0 0 16px;">
              مرحباً، <strong style="color:#e8dcc8;">{System.Net.WebUtility.HtmlEncode(applicantName)}</strong>
            </p>
            <p style="font-size:14px;color:#8899aa;margin:0 0 20px;line-height:1.8;">
              تم استلام طلب تأشيرتك بنجاح. سيتم مراجعته خلال <strong style="color:#d4af37;">24 إلى 48 ساعة</strong>
              وسيصلك إشعار بقرار المراجعة.
            </p>

            <table width="100%" cellpadding="0" cellspacing="0">
              {Row("نوع التأشيرة",     visaType)}
              {Row("دولة الوجهة",      destination)}
              {Row("رمز المرجع",       refCode)}
              {Row("وقت التقديم",      dateStr)}
            </table>
            """;

        return Wrap(
            accentColor: "#d4af37",
            icon:        "📥",
            heading:     "تم استلام طلب التأشيرة",
            body:        body,
            footer:      $"© {DateTime.UtcNow.Year} Visa Z · احتفظ برمز المرجع لمتابعة طلبك"
        );
    }

    // ── Shared table row helper ───────────────────────────────────────────────
    private static string Row(string label, string value) => $"""
        <tr>
          <td style="padding:7px 0;font-size:13px;color:#6b7a8d;border-bottom:1px solid rgba(255,255,255,0.05);
                     width:45%;">{label}</td>
          <td style="padding:7px 0;font-size:13px;color:#e8dcc8;border-bottom:1px solid rgba(255,255,255,0.05);
                     font-weight:600;">{value}</td>
        </tr>
        """;
}
