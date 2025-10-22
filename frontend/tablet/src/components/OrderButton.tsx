import React from 'react';
import './OrderButton.css';

interface OrderButtonProps {
  onClick: () => void;
  disabled?: boolean;
  loading?: boolean;
  variant?: 'primary' | 'secondary' | 'success' | 'danger';
  size?: 'small' | 'medium' | 'large';
  icon?: string;
  children: React.ReactNode;
  className?: string;
}

const OrderButton: React.FC<OrderButtonProps> = ({
  onClick,
  disabled = false,
  loading = false,
  variant = 'primary',
  size = 'medium',
  icon,
  children,
  className = ''
}) => {
  const handleClick = () => {
    if (!disabled && !loading) {
      onClick();
    }
  };

  const buttonClasses = [
    'order-button',
    `order-button--${variant}`,
    `order-button--${size}`,
    disabled ? 'order-button--disabled' : '',
    loading ? 'order-button--loading' : '',
    className
  ].filter(Boolean).join(' ');

  return (
    <button
      className={buttonClasses}
      onClick={handleClick}
      disabled={disabled || loading}
    >
      {loading && (
        <div className="order-button__spinner">
          <div className="spinner"></div>
        </div>
      )}
      
      {!loading && icon && (
        <span className="order-button__icon">{icon}</span>
      )}
      
      <span className="order-button__text">{children}</span>
    </button>
  );
};

export default OrderButton;
