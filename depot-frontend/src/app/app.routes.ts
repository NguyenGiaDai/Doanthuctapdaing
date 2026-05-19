import { Routes } from '@angular/router';

import { Dashboard } from './pages/dashboard/dashboard';
import { Containers } from './pages/containers/containers';
import { YardOperations } from './pages/yard-operations/yard-operations';
import { YardMap } from './pages/yard-map/yard-map';
import { DeliveryOrders } from './pages/delivery-orders/delivery-orders';
import { MasterData } from './pages/master-data/master-data';
import { Reports } from './pages/reports/reports';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },
  {
    path: 'dashboard',
    component: Dashboard,
  },
  {
    path: 'containers',
    component: Containers,
  },
  {
    path: 'yard-operations',
    component: YardOperations,
  },
  {
    path: 'yard-map',
    component: YardMap,
  },
  {
    path: 'delivery-orders',
    component: DeliveryOrders,
  },
  {
    path: 'master-data',
    component: MasterData,
  },
  {
    path: 'reports',
    component: Reports,
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
