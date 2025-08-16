export const routes = [
  {
    path: "/katalog",
    name: "Katalog",
    component: () => import('@/Otium/views/Katalog/Index.vue')
  },
  {
    path: "/katalog/:datum",
    name: "Katalog-Datum",
    component: () => import('@/Otium/views/Katalog/Index.vue'),
    props: true,
  },
  {
    path: "/katalog/:datum/:terminId",
    name: "Katalog-Datum-Termin",
    component: () => import('@/Otium/views/Katalog/Index.vue'),
    props: true,
  },
  {
    path: "/aufsicht",
    name: "Aufsicht",
    component: () => import('@/Otium/views/Teacher/Aufsicht.vue'),
  },
  {
    path: "/student/:studentId",
    name: "Mentee",
    component: () => import('@/Otium/views/Teacher/Mentee.vue'),
    props: true
  },
  {
    path: "/management/termin/:terminId",
    name: "Verwaltung-Termin",
    component: () => import('@/Otium/views/Management/Termin.vue'),
    props: true
  },
  {
    path: '/management',
    name: "Verwaltung",
    component: () => import('@/Otium/views/Management/OtiaOverview.vue'),
  },
  {
    path: '/management/otium/:otiumId',
    name: "Verwaltung-Otium",
    component: () => import('@/Otium/views/Management/OtiumEdit.vue'),
    props: true
  },
  {
    path: '/management/schuljahr/neu',
    name: "Verwaltung-Schuljahr-Neu",
    component: () => import('@/Otium/components/Schuljahr/CreateSchoolyear.vue')
  }
];
