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
    const savedAdminKey = localStorage.getItem(ADMIN_KEY_STORAGE);
    if (savedAdminKey) {
      // Validate the saved key
      apiService.validateAdminKey(savedAdminKey).then(isValid => {
        if (isValid) {
          dispatch({ type: 'RESTORE_SESSION', payload: savedAdminKey });
        } else {
          localStorage.removeItem(ADMIN_KEY_STORAGE);
        }
      });
    }
  }, []);

  const login = async (adminKey: string): Promise<boolean> => {
    try {
      const isValid = await apiService.validateAdminKey(adminKey);
      if (isValid) {
        localStorage.setItem(ADMIN_KEY_STORAGE, adminKey);
        dispatch({ type: 'LOGIN', payload: adminKey });
        return true;
      }
      return false;
    } catch (error) {
      console.error('Login failed:', error);
      return false;
    }
  };

  const logout = () => {
    localStorage.removeItem(ADMIN_KEY_STORAGE);
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
