import React, { useState, useEffect } from 'react';
import './ImageGallery.css';

interface ImageGalleryProps {
  images: string[];
  alt: string;
  className?: string;
}

const ImageGallery: React.FC<ImageGalleryProps> = ({ images, alt, className = '' }) => {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [loadedImages, setLoadedImages] = useState<boolean[]>([]);

  useEffect(() => {
    setLoadedImages(new Array(images.length).fill(false));
  }, [images]);

  const handleImageLoad = (index: number) => {
    setLoadedImages(prev => {
      const newLoaded = [...prev];
      newLoaded[index] = true;
      return newLoaded;
    });
  };

  const handleImageError = (index: number) => {
    setLoadedImages(prev => {
      const newLoaded = [...prev];
      newLoaded[index] = false;
      return newLoaded;
    });
  };

  const goToPrevious = () => {
    setCurrentIndex((prevIndex) => 
      prevIndex === 0 ? images.length - 1 : prevIndex - 1
    );
  };

  const goToNext = () => {
    setCurrentIndex((prevIndex) => 
      prevIndex === images.length - 1 ? 0 : prevIndex + 1
    );
  };

  const goToImage = (index: number) => {
    setCurrentIndex(index);
  };

  if (images.length === 0) {
    return (
      <div className={`image-gallery empty ${className}`}>
        <div className="placeholder-image">
          <span>📷</span>
          <p>No images available</p>
        </div>
      </div>
    );
  }

  return (
    <div className={`image-gallery ${className}`}>
      <div className="image-gallery__main">
        <div className="image-gallery__container">
          {images.map((image, index) => (
            <div
              key={index}
              className={`image-gallery__slide ${
                index === currentIndex ? 'active' : ''
              }`}
            >
              {loadedImages[index] ? (
                <img
                  src={image}
                  alt={`${alt} - Image ${index + 1}`}
                  className="image-gallery__image"
                  onError={() => handleImageError(index)}
                />
              ) : (
                <div className="image-gallery__placeholder">
                  <div className="loading-spinner"></div>
                  <p>Loading...</p>
                </div>
              )}
            </div>
          ))}
        </div>

        {images.length > 1 && (
          <>
            <button
              className="image-gallery__nav image-gallery__nav--prev"
              onClick={goToPrevious}
              aria-label="Previous image"
            >
              ‹
            </button>
            <button
              className="image-gallery__nav image-gallery__nav--next"
              onClick={goToNext}
              aria-label="Next image"
            >
              ›
            </button>
          </>
        )}

        {images.length > 1 && (
          <div className="image-gallery__indicators">
            {images.map((_, index) => (
              <button
                key={index}
                className={`image-gallery__indicator ${
                  index === currentIndex ? 'active' : ''
                }`}
                onClick={() => goToImage(index)}
                aria-label={`Go to image ${index + 1}`}
              />
            ))}
          </div>
        )}
      </div>

      {images.length > 1 && (
        <div className="image-gallery__thumbnails">
          {images.map((image, index) => (
            <button
              key={index}
              className={`image-gallery__thumbnail ${
                index === currentIndex ? 'active' : ''
              }`}
              onClick={() => goToImage(index)}
            >
              <img
                src={image}
                alt={`${alt} thumbnail ${index + 1}`}
                onLoad={() => handleImageLoad(index)}
                onError={() => handleImageError(index)}
              />
            </button>
          ))}
        </div>
      )}
    </div>
  );
};

export default ImageGallery;
