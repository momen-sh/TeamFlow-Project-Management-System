export const AppRoles = {
  Admin: 'Admin',
  TeamLeader: 'TeamLeader',
  Developer: 'Developer',
  QA: 'QA'
} as const;

export type AppRole = typeof AppRoles[keyof typeof AppRoles];

export const APP_ROLES: AppRole[] = [
  AppRoles.Admin,
  AppRoles.TeamLeader,
  AppRoles.Developer,
  AppRoles.QA
];
