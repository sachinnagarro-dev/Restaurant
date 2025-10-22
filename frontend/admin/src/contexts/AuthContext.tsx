import { createContext, useContext, useReducer, useEffect, type ReactNode } from 'react';
import type { AuthState } from '../types';
import { apiService } from '../services/apiService';

const ADMIN_KEY_STORAGE = 'admin_key';

interface AuthContextType extends AuthState {
  login: (adminKey: string) => Promise<boolean>;
  logout: () => void;
}

type AuthAction = 
  | { type: 'LOGIN'; payload: string }
  | { type: 'LOGOUT' }
  | { type: 'RESTORE_SESSION'; payload: string };

const initialState: AuthState = {
  isAuthenticated: false,
  adminKey: null,
};

function authReducer(state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case 'LOGIN':
      return {
        isAuthenticated: true,
        adminKey: action.payload,
      };
    case 'LOGOUT':
      return {
        isAuthenticated: false,
        adminKey: null,
      };
    case 'RESTORE_SESSION':
      return {
        isAuthenticated: true,
        adminKey: action.payload,
      };
    default:
      return state;
  }
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(authReducer, initialState);

  // Restore session on mount
  useEffect(() => {
    const savedToken = localStorage.getItem('jwt_token');
    if (savedToken) {
      // Validate the saved token
      apiService.setAuthToken(savedToken);
      apiService.getCurrentUser()
        .then(userInfo => {
          dispatch({ type: 'RESTORE_SESSION', payload: savedToken });
        })
        .catch(() => {
          localStorage.removeItem('jwt_token');
          apiService.clearAuthToken();
        });
    }
  }, []);

  const login = async (adminKey: string): Promise<boolean> => {
    try {
      const response = await apiService.loginWithAdminKey(adminKey);
      localStorage.setItem('jwt_token', response.token);
      dispatch({ type: 'LOGIN', payload: response.token });
      return true;
    } catch (error) {
      console.error('Login failed:', error);
      return false;
    }
  };

  const logout = () => {
    localStorage.removeItem('jwt_token');
    apiService.clearAuthToken();
    dispatch({ type: 'LOGOUT' });
  };

  const value: AuthContextType = {
    ...state,
    login,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
