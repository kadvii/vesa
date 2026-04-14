export const mockVisaStats = [
  { label: 'إجمالي طلبات الفيزا', value: 18, hint: 'خلال آخر 30 يوم', recent: [
    { id: 'v-01', country: 'الإمارات', date: '2026-04-04', status: 'قيد المراجعة', badgeClass: 'bg-warning/20 text-warning border border-warning/40' },
    { id: 'v-02', country: 'تركيا', date: '2026-04-02', status: 'مقبول', badgeClass: 'bg-success/20 text-success border border-success/40' },
    { id: 'v-03', country: 'الأردن', date: '2026-03-30', status: 'مرفوض', badgeClass: 'bg-danger/20 text-danger border border-danger/40' },
  ] },
  { label: 'قيد المراجعة', value: 6, hint: 'ينتظر الاعتماد' },
  { label: 'مقبولة', value: 9, hint: 'جاهزة للتسليم' },
  { label: 'مرفوضة', value: 3, hint: 'تحتاج إعادة تقديم' },
];
