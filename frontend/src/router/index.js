import { createRouter, createWebHistory } from 'vue-router'

import Home from '../pages/Home.vue'
import Login from '../pages/Login.vue'
import Question from '../pages/Question.vue'
import AdminLogin from '../pages/AdminLogin.vue'
import AdminDashboard from '../pages/AdminDashboard.vue'
import AdminMemberList from '../pages/AdminMemberList.vue'
import AdminAnswerList from '../pages/AdminAnswerList.vue'
import AdminExport from '../pages/AdminExport.vue'
import AdminCredit from '../pages/AdminCredit.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home,
  },
  {
    path: '/login',
    name: 'Login',
    component: Login,
  },
  {
    path: '/question',
    name: 'Question',
    component: Question,
  },
  {
    path: '/admin/login',
    name: 'AdminLogin',
    component: AdminLogin,
  },
  {
    path: '/admin',
    redirect: '/admin/dashboard',
  },
  {
    path: '/admin/dashboard',
    name: 'AdminDashboard',
    component: AdminDashboard,
    meta: {
      requiresAdminAuth: true,
    },
  },
  {
    path: '/admin/members',
    name: 'AdminMemberList',
    component: AdminMemberList,
    meta: {
      requiresAdminAuth: true,
    },
  },
  {
    path: '/admin/answers',
    name: 'AdminAnswerList',
    component: AdminAnswerList,
    meta: {
      requiresAdminAuth: true,
    },
  },
  {
    path: '/admin/export',
    name: 'AdminExport',
    component: AdminExport,
    meta: {
      requiresAdminAuth: true,
    },
  },
  {
    path: '/admin/credit',
    name: 'AdminCredit',
    component: AdminCredit,
    meta: {
      requiresAdminAuth: true,
    },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to) => {
  const requiresAdminAuth = to.matched.some(
    (record) => record.meta.requiresAdminAuth
  )

  const adminId = localStorage.getItem('adminId')
  const adminUsername = localStorage.getItem('adminUsername')

  if (requiresAdminAuth && (!adminId || !adminUsername)) {
    return '/admin/login'
  }

  if (to.path === '/admin/login' && adminId && adminUsername) {
    return '/admin/dashboard'
  }

  return true
})

export default router