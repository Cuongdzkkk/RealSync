import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';

import { requireAuth } from './guards';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('@/components/layout/PublicLayout.vue'),
    children: [
      {
        path: '',
        name: 'marketplace',
        component: () => import('@/views/public/MarketplaceView.vue'),
        meta: { title: 'Thị trường' }
      }
    ]
  },
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/auth/LoginView.vue'),
    meta: { title: 'Đăng nhập' }
  },
  {
    path: '/admin',
    component: () => import('@/components/layout/AdminLayout.vue'),
    beforeEnter: requireAuth,
    children: [
      {
        path: '',
        redirect: '/admin/dashboard'
      },
      {
        path: 'dashboard',
        name: 'dashboard',
        component: () => import('@/views/dashboard/DashboardView.vue'),
        meta: { title: 'Dashboard' }
      },
      {
        path: 'properties',
        name: 'properties',
        component: () => import('@/views/properties/PropertyListView.vue'),
        meta: { title: 'Sản phẩm' }
      },
      {
        path: 'projects',
        name: 'projects',
        component: () => import('@/views/projects/ProjectListView.vue'),
        meta: { title: 'Dự án' }
      },
      {
        path: 'properties/:id',
        name: 'property-detail',
        component: () => import('@/views/properties/PropertyDetailView.vue'),
        meta: { title: 'Chi tiết sản phẩm' }
      },
      {
        path: 'leads',
        name: 'leads',
        component: () => import('@/views/leads/LeadListView.vue'),
        meta: { title: 'Leads' }
      },
      {
        path: 'crawlers',
        name: 'crawlers',
        component: () => import('@/views/crawlers/CrawlerView.vue'),
        meta: { title: 'Crawler' }
      },
      {
        path: 'ai-classification',
        name: 'ai-classification',
        component: () => import('@/views/ai/AiClassificationView.vue'),
        meta: { title: 'AI Classification' }
      },
      {
        path: 'content-ai',
        name: 'content-ai',
        component: () => import('@/views/content/ContentAiView.vue'),
        meta: { title: 'Content AI' }
      },
      {
        path: 'insights',
        name: 'insights',
        component: () => import('@/views/insights/InsightView.vue'),
        meta: { title: 'Insight' }
      },
      {
        path: 'users',
        name: 'users',
        component: () => import('@/views/users/UserManagementView.vue'),
        meta: { title: 'Users & Roles' }
      },
      {
        path: 'settings',
        name: 'settings',
        component: () => import('@/views/settings/SettingsView.vue'),
        meta: { title: 'Cài đặt' }
      }
    ]
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 })
});

router.afterEach((to) => {
  document.title = `${String(to.meta.title ?? 'App')} | RealSync`;
});

export default router;
