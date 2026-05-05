import { Routes } from '@angular/router';

import { Dashboard } from './pages/dashboard/dashboard';
import { Containers } from './pages/containers/containers';
import { ImportContainer } from './pages/import-container/import-container';
import { ExportContainer } from './pages/export-container/export-container';
import { Statistics } from './pages/statistics/statistics';
import { MasterData } from './pages/master-data/master-data';

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
    path: 'import-container',
    component: ImportContainer,
  },
  {
    path: 'export-container',
    component: ExportContainer,
  },
  {
    path: 'statistics',
    component: Statistics,
  },
  {
    path: 'master-data',
    component: MasterData,
  },
];
