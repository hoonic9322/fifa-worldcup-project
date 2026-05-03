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
    name: 'AdminDashboard',
    component: AdminDashboard,
  },
  {
  path: '/admin/members',
  name: 'AdminMemberList',
  component: AdminMemberList,
  },
  {
  path: '/admin/answers',
  name: 'AdminAnswerList',
  component: AdminAnswerList,
  },
  {
  path: '/admin/export',
  name: 'AdminExport',
  component: AdminExport,
},
{
  path: '/admin/credit',
  name: 'AdminCredit',
  component: AdminCredit,
},
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router