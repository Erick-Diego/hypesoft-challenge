import Keycloak from 'keycloak-js';

const keycloakConfig = {
  url: import.meta.env.VITE_KEYCLOAK_URL || 'http://localhost:8080',
  realm: import.meta.env.VITE_KEYCLOAK_REALM || 'hypesoft',
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID || 'hypesoft-frontend',
};

export const keycloak = new Keycloak(keycloakConfig);

export const initKeycloak = async () => {
  try {
    const authenticated = await keycloak.init({
      onLoad: 'login-required',
      checkLoginIframe: false,
      pkceMethod: 'S256',
      enableLogging: true, 
    });

    if (authenticated) {
        console.log('✅ User authenticated');

        console.log('🔐 Access Token:');
        console.log(keycloak.token);

        console.log('📦 Parsed Token:');
        console.table(keycloak.tokenParsed);
    }

    return authenticated;
  } catch (error) {
    console.error('Keycloak init error:', error);
    return false;
  }
};

export const hasRole = (role: string): boolean => {
  return keycloak.hasRealmRole(role);
};

export const hasAnyRole = (roles: string[]): boolean => {
  return roles.some(role => keycloak.hasRealmRole(role));
};

export const getUserInfo = () => {
  return {
    username: keycloak.tokenParsed?.preferred_username,
    email: keycloak.tokenParsed?.email,
    name: keycloak.tokenParsed?.name,
    roles: keycloak.realmAccess?.roles || [],
  };
};

export const getToken = () => {
  return keycloak.token;
};

export const logout = () => {
  keycloak.logout({ redirectUri: window.location.origin });
};