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

import Campaign20K from '../pages/campaign/Campaign20K.vue'
import Campaign80KLogin from '../pages/campaign/Campaign80KLogin.vue'
import Campaign80KHowToWin from '../pages/campaign/Campaign80KHowToWin.vue'
import Campaign80KQuestions from '../pages/campaign/Campaign80KQuestions.vue'
import FifaSchedule from '../pages/campaign/FifaSchedule.vue'
import FifaResult from '../pages/campaign/FifaResult.vue'

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

  // Public Campaign Pages
  {
    path: '/campaign/20k',
    name: 'Campaign20K',
    component: Campaign20K,
  },
  {
    path: '/campaign/80k/login',
    name: 'Campaign80KLogin',
    component: Campaign80KLogin,
  },
  {
    path: '/campaign/80k/how-to-win',
    name: 'Campaign80KHowToWin',
    component: Campaign80KHowToWin,
  },
  {
    path: '/campaign/80k/questions',
    name: 'Campaign80KQuestions',
    component: Campaign80KQuestions,
  },
  {
    path: '/campaign/fifa-schedule',
    name: 'FifaSchedule',
    component: FifaSchedule,
  },
  {
    path: '/campaign/fifa-result',
    name: 'FifaResult',
    component: FifaResult,
  },

  // Admin Pages
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